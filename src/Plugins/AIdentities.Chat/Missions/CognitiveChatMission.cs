using AIdentities.Chat.AIdentiy;
using AIdentities.Chat.Skills.IntroduceYourself;
using AIdentities.Chat.Skills.ReplyToPrompt;
using AIdentities.Shared.Features.CognitiveEngine;
using AIdentities.Shared.Features.CognitiveEngine.Mission;
using AIdentities.Shared.Features.CognitiveEngine.Skills;
using AIdentities.Shared.Features.Core.Services;

namespace AIdentities.Chat.Missions;

/// <summary>
/// To generate a custom mission for specific needs, it's easier to just inherit from the Mission class
/// and implement details of the mission within, like constraints, goals and maybe custom state properties that map to the mission context.
/// In this example we'll register the mission in the DI container and use it in the CognitiveChat page.
/// </summary>
internal class CognitiveChatMission : Mission<CognitiveChatMissionContext>
{
   readonly ILogger<CognitiveChatMission> _logger;
   readonly IAIdentityProvider _aIdentityProvider;
   readonly ISkillManager _skillManager;
   readonly ICognitiveEngineProvider _cognitiveEngineProvider;
   readonly ChatSettings _settings;

   /// <summary>
   /// The AIdentity that is running the mission.
   /// </summary>
   public AIdentity MissionRunner { get; } = new ChatKeeper();

   public CognitiveChatMission(ILogger<CognitiveChatMission> logger,
                               IPluginSettingsManager pluginSettingsManager,
                               IAIdentityProvider aIdentityProvider,
                               ISkillManager skillManager,
                               ICognitiveEngineProvider cognitiveEngineProvider)
   {
      _logger = logger;
      _aIdentityProvider = aIdentityProvider;
      _skillManager = skillManager;
      _cognitiveEngineProvider = cognitiveEngineProvider;
      _settings = pluginSettingsManager.Get<ChatSettings>();

      SetupMission(_settings);
   }

   private void SetupMission(ChatSettings settings)
   {
      Goal = "Handle the conversation between AIdentities and humans, let them chat freely and assist anytime there is a skill you know to execute";

      if (settings.EnableSkills)
      {
         foreach (var skillName in settings.EnabledSkills)
         {
            var skill = _skillManager.Get(skillName);
            if (skill is null)
            {
               _logger.LogWarning("The skill {skillName} is not available and will not be used.", skillName);
               continue;
            }

            Context.SkillConstraints.Add(new SkillConstraint(skill, MissionRunner));
         }
      }

      Context.AIdentitiesConstraints.AllowedAIdentities.Add(MissionRunner);
   }


   public void ClearConversation()
   {
      Context.CurrentConversation = null;
      Context.PartecipatingAIdentities.Clear();
   }

   /// <summary>
   /// We want to start a new conversation.
   /// We clear previous conversation data and start the conversation from the point we left the conversation we are going to load.
   /// If the conversation is empty, we are going to ask the first AIdentity that participate to the discussion, to start talking.
   /// </summary>
   /// <param name="conversation"></param>
   public async IAsyncEnumerable<Thought> StartNewConversationAsync(Conversation conversation)
   {
      Context.CurrentConversation = conversation;
      Context.PartecipatingAIdentities.Clear();

      foreach (var aidentityId in conversation.Metadata.AIdentityIds)
      {
         var aidentity = _aIdentityProvider.Get(aidentityId);
         if (aidentity is null)
         {
            _logger.LogWarning("The AIdentity {aidentityId} is not available and will not be used.", aidentityId);
            continue;
         }

         // we create a new cognitive engine for each AIdentity that participate to the conversation.
         Context.PartecipatingAIdentities.Add(
            aidentityId,
            new PartecipatingAIdentity(_cognitiveEngineProvider.CreateCognitiveEngine(aidentity))
            );
      }

      if (conversation.Messages is not { Count: > 0 })
      {
         // if the conversation is empty, we ask the first AIdentity that participate to the discussion, to start talking.
         var firstParticipant = Context.PartecipatingAIdentities.First().Value.CognitiveEngine;

         // we bypass mission constraints on this skill because it's needed to start a conversation.
         var replyToPromptSkill = _skillManager.Get<IntroduceYourself>();
         if (replyToPromptSkill is null)
         {
            _logger.LogWarning("The skill {skillName} is not available and will not be used.", nameof(IntroduceYourself));
            yield break;
         }

         var skillExecutionContext = new SkillExecutionContext(
            replyToPromptSkill,
            firstParticipant.Context,
            Context
            );

         var responseStream = replyToPromptSkill.ExecuteAsync(
            new InstructionPrompt("Hello"),
            skillExecutionContext,
            Context.MissionRunningCancellationToken).ConfigureAwait(false);

         await foreach (var item in responseStream)
         {
            yield return item;
         }
      }
   }
}

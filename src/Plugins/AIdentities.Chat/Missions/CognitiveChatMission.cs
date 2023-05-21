using AIdentities.Chat.AIdentiy;
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
   readonly ISkillManager _skillManager;
   readonly ChatSettings _settings;

   /// <summary>
   /// The AIdentity that is running the mission.
   /// </summary>
   public AIdentity MissionRunner { get; } = new ChatKeeper();

   public CognitiveChatMission(ILogger<CognitiveChatMission> logger, IPluginSettingsManager pluginSettingsManager, ISkillManager skillManager)
   {
      _logger = logger;
      _skillManager = skillManager;
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
   /// If the conversation is empty, we are going to ask the first AIdentity that partecipate to the discussion, to start talking.
   /// </summary>
   /// <param name="conversation"></param>
   public void StartNewConversation(Conversation conversation)
   {
      Context.CurrentConversation = conversation;
      Context.PartecipatingAIdentities = new HashSet<AIdentity>();
   }
}

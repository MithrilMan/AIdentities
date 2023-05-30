using System.Threading.Channels;
using AIdentities.Chat.CognitiveEngine;
using AIdentities.Chat.Skills.IntroduceYourself;
using AIdentities.Chat.Skills.InviteFriend.Events;
using AIdentities.Shared.Features.CognitiveEngine.Engines.Conversational;
using AIdentities.Shared.Services.EventBus;

namespace AIdentities.Chat.Missions;

/// <summary>
/// To generate a custom mission for specific needs, it's easier to just inherit from the Mission class
/// and implement details of the mission within, like constraints, goals and maybe custom state properties that map to the mission context.
/// In this example we'll register the mission in the DI container and use it in the CognitiveChat page.
/// </summary>
internal class CognitiveChatMission : Mission<CognitiveChatMissionContext>,
   IHandle<InviteToConversation>
{
   readonly ILogger<CognitiveChatMission> _logger;
   readonly IAIdentityProvider _aIdentityProvider;
   readonly ISkillManager _skillManager;
   readonly ICognitiveEngineProvider _cognitiveEngineProvider;
   readonly IEventBus _eventBus;
   readonly ChatSettings _settings;

   /// <summary>
   /// This channel is used to communicate the thoughts as they arrive from cognitive engines working on the mission.
   /// It's configured to only have one consumer and one producer, and to drop the oldest thought if the channel is full.
   /// </summary>
   public Channel<Thought> Thoughts { get; } = Channel.CreateBounded<Thought>(new BoundedChannelOptions(1000)
   {
      FullMode = BoundedChannelFullMode.DropOldest,
      SingleReader = true,
      SingleWriter = true
   });

   /// <summary>
   /// The AIdentity responsible for keeping the conversation.
   /// </summary>
   public AIdentity ChatKeeper { get; set; } = new ChatKeeper();

   public CognitiveChatMission(ILogger<CognitiveChatMission> logger,
                               IPluginSettingsManager pluginSettingsManager,
                               IAIdentityProvider aIdentityProvider,
                               ISkillManager skillManager,
                               ICognitiveEngineProvider cognitiveEngineProvider,
                               IEventBus eventBus)
   {
      _logger = logger;
      _aIdentityProvider = aIdentityProvider;
      _skillManager = skillManager;
      _cognitiveEngineProvider = cognitiveEngineProvider;
      _eventBus = eventBus;
      _settings = pluginSettingsManager.Get<ChatSettings>();

      _eventBus.Subscribe(this);

      SetupMission(_settings);
   }

   private void SetupMission(ChatSettings settings)
   {
      Goal = "Handle the conversation between AIdentities and humans, let them chat freely and assist anytime there is a skill you know to execute";

      if (settings.EnableSkills)
      {
         foreach (var skillName in settings.EnabledSkills)
         {
            var skill = _skillManager.GetSkillDefinition(skillName);
            if (skill is null)
            {
               _logger.LogWarning("The skill {skillName} is not available and will not be used.", skillName);
               continue;
            }

            Context.SkillConstraints.Add(new SkillConstraint(skill, ChatKeeper));
         }
      }

      Context.AIdentitiesConstraints.AllowedAIdentities.Add(ChatKeeper);
   }


   public void Start(CancellationToken cancellationToken)
   {
      Start(_cognitiveEngineProvider.CreateCognitiveEngine<ChatKeeperCognitiveEngine>(ChatKeeper), cancellationToken);
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
   public async Task StartNewConversationAsync(Conversation conversation)
   {
      Context.CurrentConversation = conversation;
      Context.PartecipatingAIdentities.Clear();

      foreach (var aidentityId in conversation.AIdentityIds)
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
            new PartecipatingAIdentity(_cognitiveEngineProvider.CreateCognitiveEngine<ChatCognitiveEngine>(aidentity))
            );
      }

      if (conversation.Messages is not { Count: > 0 })
      {
         // if the conversation is empty, we ask the first AIdentity that participate to the discussion, to start talking.
         var firstParticipant = Context.PartecipatingAIdentities.First().Value.CognitiveEngine;

         await MakeAIdentityIntroduce(firstParticipant).ConfigureAwait(false);
      }
   }

   private async Task MakeAIdentityIntroduce(ICognitiveEngine whoToIntroduce)
   {
      // we bypass mission constraints on this skill because it's needed to start a conversation.
      var replyToPromptSkill = _skillManager.Get<IntroduceYourself>();
      if (replyToPromptSkill is null)
      {
         _logger.LogWarning("The skill {skillName} is not available and will not be used.", nameof(IntroduceYourself));
         return;
      }

      var skillExecutionContext = new SkillExecutionContext(
         replyToPromptSkill,
         whoToIntroduce.Context,
         Context
         );

      await EnqueueThoughtsAsync(replyToPromptSkill.ExecuteAsync(
         new AIdentityPrompt(ChatKeeper.Id, "Introduce Yourself"),
         skillExecutionContext,
         Context.MissionRunningCancellationToken)).ConfigureAwait(false);
   }

   private async Task EnqueueThoughtsAsync(IAsyncEnumerable<Thought> thoughts)
   {
      var items = thoughts.ConfigureAwait(false);
      await foreach (var item in items)
      {
         Thoughts.Writer.TryWrite(item);
      }
   }

   public override void Dispose()
   {
      _eventBus.Unsubscribe(this);
      base.Dispose();
   }


   /// <summary>
   /// When a new AIdentity is invited to the conversation, we create a new cognitive engine for it.
   /// This makes use of event bus to be notified when a new AIdentity is invited to the conversation.
   /// The event is raised by the <see cref="Skills.InviteFriend.InviteFriend"/>skill.
   /// </summary>
   /// <param name="message"></param>
   /// <returns></returns>
   public async Task HandleAsync(InviteToConversation message)
   {
      var aidentity = message.AIdentity;
      if (!Context.PartecipatingAIdentities.ContainsKey(aidentity.Id))
      {
         // all the AIdentities that participate to the conversation are using ConversationalCognitiveEngine
         var partecipatingAIdentity = new PartecipatingAIdentity(_cognitiveEngineProvider.CreateCognitiveEngine<ChatCognitiveEngine>(aidentity));
         // we create a new cognitive engine for each AIdentity that participate to the conversation.
         Context.PartecipatingAIdentities.Add(aidentity.Id, partecipatingAIdentity);

         await MakeAIdentityIntroduce(partecipatingAIdentity.CognitiveEngine).ConfigureAwait(false);
      }
   }
}

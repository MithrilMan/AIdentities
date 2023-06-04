using System.Threading.Channels;
using AIdentities.Chat.CognitiveEngine;
using AIdentities.Chat.Skills.IntroduceYourself;
using AIdentities.Chat.Skills.InviteToChat.Events;
using AIdentities.Shared.Features.CognitiveEngine.Engines.Conversational;
using AIdentities.Shared.Features.CognitiveEngine.Memory.Conversation;
using AIdentities.Shared.Plugins.Connectors.Completion;
using AIdentities.Shared.Services.EventBus;
using Polly;

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
   readonly IEnumerable<IConversationalConnector> _conversationalConnectors;
   readonly IEnumerable<ICompletionConnector> _completionConnectors;
   readonly IConversationHistory _conversationHistory;
   readonly ChatSettings _settings;
   private readonly ICompletionConnector _defaultCompletionConnector;
   private readonly IConversationalConnector _defaultConversationalConnector;

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
   public Conversation? CurrentConversation => Context.CurrentConversation;

   public IEnumerable<AIdentity> ParticipatingAIdentities => Context.ParticipatingAIdentities.Select(p => p.Value.AIdentity);

   public CognitiveChatMission(ILogger<CognitiveChatMission> logger,
                               IPluginSettingsManager pluginSettingsManager,
                               IAIdentityProvider aIdentityProvider,
                               ISkillManager skillManager,
                               ICognitiveEngineProvider cognitiveEngineProvider,
                               IEventBus eventBus,
                               IEnumerable<IConversationalConnector> conversationalConnectors,
                               IEnumerable<ICompletionConnector> completionConnectors,
                               IConversationHistory conversationHistory)
   {
      _logger = logger;
      _aIdentityProvider = aIdentityProvider;
      _skillManager = skillManager;
      _cognitiveEngineProvider = cognitiveEngineProvider;
      _eventBus = eventBus;
      _conversationalConnectors = conversationalConnectors;
      _completionConnectors = completionConnectors;
      _conversationHistory = conversationHistory;
      _settings = pluginSettingsManager.Get<ChatSettings>();

      _defaultCompletionConnector = _completionConnectors.FirstOrDefault(c => c.Enabled && c.Name == _settings.DefaultConnector)
            ?? _completionConnectors.FirstOrDefault(c => c.Enabled) ?? throw new InvalidOperationException("No completion connector is enabled");

      _defaultConversationalConnector = _conversationalConnectors.FirstOrDefault(c => c.Enabled && c.Name == _settings.DefaultConnector)
         ?? _conversationalConnectors.FirstOrDefault(c => c.Enabled) ?? throw new InvalidOperationException("No conversational connector is enabled");

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

      //populate the missionContext with the chat history
      Context.ConversationHistory = _conversationHistory;
   }


   public void Start(CancellationToken cancellationToken)
   {
      Start(_cognitiveEngineProvider.CreateCognitiveEngine<ChatKeeperCognitiveEngine>(
         ChatKeeper,
         configure =>
         {
            configure.SetDefaultCompletionConnector(_defaultCompletionConnector);
            configure.SetDefaultConversationalConnector(_defaultConversationalConnector);
         }
         ), cancellationToken);
   }


   public void ClearConversation()
   {
      _conversationHistory.SetConversation(null);
      Context.CurrentConversation = null;
      Context.ParticipatingAIdentities.Clear();
   }

   /// <summary>
   /// We want to start a new conversation.
   /// We clear previous conversation data and start the conversation from the point we left the conversation we are going to load.
   /// If the conversation is empty, we are going to ask the first AIdentity that participate to the discussion, to start talking.
   /// </summary>
   /// <param name="conversation"></param>
   public async Task StartNewConversationAsync(Conversation conversation)
   {
      _conversationHistory.SetConversation(conversation);

      Context.CurrentConversation = conversation;
      Context.ParticipatingAIdentities.Clear();

      foreach (var aidentityId in conversation.AIdentityIds)
      {
         AddParticipant(aidentityId, false);
      }

      Context.NextTalker = Context.ParticipatingAIdentities.Count > 0 ? Context.ParticipatingAIdentities.FirstOrDefault().Value.AIdentity : null;

      if (conversation.Messages is not { Count: > 0 })
      {
         // if the conversation is empty, we ask the first AIdentity that participate to the discussion, to start talking.
         var firstParticipant = Context.ParticipatingAIdentities.First().Value.CognitiveEngine;

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
   /// The event is raised by the <see cref="Skills.InviteToChat.InviteToChat"/>skill.
   /// </summary>
   /// <param name="message"></param>
   /// <returns></returns>
   public async Task HandleAsync(InviteToConversation message)
   {
      var aidentity = message.AIdentity;
      await AddParticipant(aidentity.Id, makeAIdentityIntroduceItself: true).ConfigureAwait(false);
   }

   public void SetNextTalker(AIdentity aIdentity)
   {
      Context.NextTalker = aIdentity;
      Thoughts.Writer.TryWrite(new ActionThought(null, ChatKeeper, $"The next talker will be {aIdentity.Name}"));
   }

   public void SetModeratedMode(bool moderatedMode)
   {
      Context.IsModeratedModeEnabled = moderatedMode;
      Thoughts.Writer.TryWrite(new ActionThought(null, ChatKeeper, $"Moderated mode is now {(moderatedMode ? "enabled" : "disabled")}"));
   }

   public async Task ReplyToMessageAsync(ConversationMessage? conversationMessage)
   {
      Context.MessageToReplyTo = conversationMessage;
      if (conversationMessage is null) return;

      try
      {
         await EnqueueThoughtsAsync(MissionRunner!.HandlePromptAsync(new AIdentityPrompt(
            ChatKeeper.Id,
            $"{Context.MessageToReplyTo!.AuthorName}, reply to message above"
            ), Context, Context.MissionRunningCancellationToken)).ConfigureAwait(false);
      }
      catch
      {
         throw;
      }
      finally
      {
         Context.MessageToReplyTo = null;
      }
   }

   public async Task AddParticipant(Guid aIdentityId, bool makeAIdentityIntroduceItself)
   {
      if (Context.CurrentConversation is null) throw new InvalidOperationException("The conversation is not started.");

      var aidentity = _aIdentityProvider.Get(aIdentityId);
      if (aidentity is null)
      {
         _logger.LogWarning("The AIdentity {aidentityId} is not available and will not be used.", aIdentityId);
         return;
      }

      if (!Context.ParticipatingAIdentities.ContainsKey(aidentity.Id))
      {
         // all the AIdentities that participate to the conversation are using ConversationalCognitiveEngine
         var cognitiveEngine = _cognitiveEngineProvider.CreateCognitiveEngine<ChatCognitiveEngine>(aidentity, configure: (c) =>
         {
            c.SetDefaultCompletionConnector(_defaultCompletionConnector);
            c.SetDefaultConversationalConnector(_defaultConversationalConnector);
         });
         var participatingAIdentity = new ParticipatingAIdentity(cognitiveEngine);

         Context.CurrentConversation.AddAIdentity(aidentity);
         // we create a new cognitive engine for each AIdentity that participate to the conversation.
         Context.ParticipatingAIdentities.Add(
            aIdentityId,
            participatingAIdentity
            );

         if (makeAIdentityIntroduceItself)
         {
            await MakeAIdentityIntroduce(participatingAIdentity.CognitiveEngine).ConfigureAwait(false);
         }
      }
   }
}

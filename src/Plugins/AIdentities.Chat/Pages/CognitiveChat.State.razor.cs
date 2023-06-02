using System.Diagnostics.CodeAnalysis;
using AIdentities.Shared.Plugins.Connectors.TextToSpeech;
using AIdentities.Shared.Services.Javascript;

namespace AIdentities.Chat.Pages;

public partial class CognitiveChat
{
   class State
   {
      /// <summary>
      /// True if no conversation is selected.
      /// </summary>
      [MemberNotNullWhen(false, nameof(CurrentConversation))]
      public bool NoConversation => CognitiveChatMission.CurrentConversation is null;

      /// <summary>
      /// returns the current conversation.
      /// </summary>
      public Conversation? CurrentConversation => CognitiveChatMission.CurrentConversation;

      /// <summary>
      /// Current participating AIdentities.
      /// </summary>
      public IEnumerable<AIdentity> ParticipatingAIdentities => CognitiveChatMission.ParticipatingAIdentities;

      /// <summary>
      /// True if the user can moderate the conversation.
      /// Moderating a conversation means being able to chose who have to talk/reply to a message.
      /// In moderation mode the user can even send a message specifying who has to reply to it.
      /// </summary>
      public bool IsModeratorModeEnabled { get; set; }

      public string? Message { get; set; }
      public ConversationMessage? SelectedMessage { get; set; } = default!;

      /// <summary>
      /// This is used to prevent the user from sending multiple messages before the first one has been replied to.
      /// When the user send a message, this is set to true, and when the message is replied to, it is set to false.
      /// </summary>
      public bool IsWaitingReply { get; set; }

      public bool CanSendMessage => Connector != null && !IsWaitingReply && !NoConversation;

      public bool HasMessageGenerationFailed => !CurrentConversation?.Messages.LastOrDefault()?.IsAIGenerated ?? false; //{ get; internal set; }

      /// <summary>
      /// Holds the reference to the current Conversational Connector.
      /// If null, no chat message can be sent.
      /// </summary>
      public IConversationalConnector? Connector { get; set; }

      /// <summary>
      /// Holds the reference to the default Text To Speech Connector.
      /// Depending on the installed Text To Speech Connectors, an AIdentity can override this value
      /// and use its own connector with its own voice.
      /// </summary>
      public ITextToSpeechConnector? TextToSpeechConnector { get; internal set; }

      /// <summary>
      /// The service used to play audio streams.
      /// </summary>
      public IPlayAudioStream PlayAudioStream { get; private set; } = default!;

      public string ParticipatingAIdentitiesTooltip => string.Join(", ", CognitiveChatMission.ParticipatingAIdentities.Select(aidentity => aidentity.Name));

      public CognitiveChatMission CognitiveChatMission { get; private set; } = default!;

      public IAIdentityProvider AIdentityProvider { get; private set; } = default!;

      /// <summary>
      /// The cancellation token source used to cancel the message generation.
      /// </summary>
      public CancellationTokenSource MessageGenerationCancellationTokenSource { get; set; } = new CancellationTokenSource();
      public CancellationTokenSource PlayingSpeechCancellationTokenSource { get; set; } = new CancellationTokenSource();

      public List<Thought> ChatKeeperThoughts { get; } = new();

      /// <summary>
      /// When the chat keeper is thinking, this is set to true for a bit.
      /// </summary>
      public bool IsChatKeeperThinking { get; set; }

      public void Initialize(CognitiveChatMission cognitiveChatMission, IAIdentityProvider aidentityProvider, IPlayAudioStream playAudioStream)
      {
         CognitiveChatMission = cognitiveChatMission;
         AIdentityProvider = aidentityProvider;
         PlayAudioStream = playAudioStream;
      }

      public async Task InitializeConversation(Conversation conversation)
      {
         await PlayAudioStream.StopAudioFiles().ConfigureAwait(false);
         await CognitiveChatMission.StartNewConversationAsync(conversation).ConfigureAwait(false);
         SelectedMessage = conversation.Messages.FirstOrDefault();
      }

      public void CloseConversation()
      {
         _ = PlayAudioStream.StopAudioFiles();
         ChatKeeperThoughts.Clear();
         CognitiveChatMission.ClearConversation();
      }
   }

   private readonly State _state = new State();
}

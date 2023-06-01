﻿using AIdentities.Shared.Plugins.Connectors.TextToSpeech;

namespace AIdentities.Chat.Pages;

public partial class CognitiveChat
{
   class State
   {
      public string? MessageSearchText { get; set; }

      public Conversation? SelectedConversation { get; set; }
      /// <summary>
      /// True if no conversation is selected.
      /// </summary>
      public bool NoConversation => SelectedConversation is null;

      /// <summary>
      /// True if the user can moderate the conversation.
      /// Moderating a conversation means being able to chose who have to talk/reply to a message.
      /// In moderation mode the user can even send a message specifying who has to reply to it.
      /// </summary>
      public bool IsModeratorModeEnabled { get; set; }

      public string? Message { get; set; }
      public ConversationMessage? SelectedMessage { get; set; } = default!;
      public FilteredObservableCollection<ConversationMessage> Messages { get; private set; } = default!;

      /// <summary>
      /// This is used to prevent the user from sending multiple messages before the first one has been replied to.
      /// When the user send a message, this is set to true, and when the message is replied to, it is set to false.
      /// </summary>
      public bool IsWaitingReply { get; set; }

      public bool CanSendMessage => Connector != null && !IsWaitingReply && SelectedConversation != null;

      public bool HasMessageGenerationFailed { get; internal set; }

      /// <summary>
      /// The response from the chat API, streamed as it comes in.
      /// During the streaming, this is used to store the response, and when the streaming is done, it is 
      /// cleared and the response is added to the <see cref="Messages"/> collection.
      /// </summary>
      public ConversationMessage? StreamedResponse { get; set; }

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
      /// A collection of all the AIdentities that are participating in the current conversation.
      /// </summary>
      public HashSet<AIdentity> ParticipatingAIdentities { get; set; } = new HashSet<AIdentity>();

      public string ParticipatingAIdentitiesTooltip => string.Join(", ", ParticipatingAIdentities.Select(aidentity => aidentity.Name));

      public IAIdentityProvider AIdentityProvider { get; private set; } = default!;

      /// <summary>
      /// The cancellation token source used to cancel the message generation.
      /// </summary>
      public CancellationTokenSource MessageGenerationCancellationTokenSource { get; set; } = new CancellationTokenSource();

      public List<Thought> ChatKeeperThoughts { get; } = new();

      /// <summary>
      /// When the chat keeper is thinking, this is set to true for a bit.
      /// </summary>
      public bool IsChatKeeperThinking { get; set; }


      public void Initialize(Func<IEnumerable<ConversationMessage>, ValueTask<IEnumerable<ConversationMessage>>> messageFilter, IAIdentityProvider aidentityProvider)
      {
         Messages = new(messageFilter);
         AIdentityProvider = aidentityProvider;
         MessageSearchText = null;
      }

      public async Task InitializeConversation(Conversation conversation, bool loadMessages = true)
      {
         SelectedConversation = conversation;
         // if the last message is not generated, we need to generate a reply so we enable the "resend" button
         HasMessageGenerationFailed = conversation.Messages?.LastOrDefault()?.IsAIGenerated == false;
         if (loadMessages)
         {
            await Messages.LoadItemsAsync(conversation.Messages).ConfigureAwait(false);
         }

         ParticipatingAIdentities = conversation.AIdentityIds
            .Select(AIdentityProvider.Get)
            .Where(aidentity => aidentity is not null)
            .Select(aidentity => aidentity!)
            .ToHashSet();
      }

      public async Task CloseConversation()
      {
         SelectedConversation = null;
         HasMessageGenerationFailed = false;
         await Messages.LoadItemsAsync(null).ConfigureAwait(false);

         ParticipatingAIdentities.Clear();
      }
   }

   private readonly State _state = new State();
}

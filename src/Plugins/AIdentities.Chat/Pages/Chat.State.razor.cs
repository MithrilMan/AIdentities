using AIdentities.Shared.Plugins.Connectors.Conversational;

namespace AIdentities.Chat.Pages;

public partial class Chat
{
   class State
   {
      public string? MessageSearchText { get; set; }
      public ConversationMetadata? SelectedConversation { get; set; }
      public string? Message { get; set; }
      public ChatMessage? SelectedMessage { get; set; } = default!;
      public FilteredObservableCollection<ChatMessage> Messages { get; private set; } = default!;

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
      public ChatMessage? StreamedResponse { get; set; }

      /// <summary>
      /// Holds the reference to the current Conversational Connector.
      /// If null, no chat message can be sent.
      /// </summary>
      public IConversationalConnector? Connector { get; set; }

      public void Initialize(Func<IEnumerable<ChatMessage>, ValueTask<IEnumerable<ChatMessage>>> messageFilter)
      {
         MessageSearchText = null;
         Messages = new(messageFilter);
      }
   }

   private readonly State _state = new State();
}

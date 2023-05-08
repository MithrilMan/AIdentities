using System.Text;

namespace AIdentities.Chat.Pages;

public partial class Chat
{
   class State
   {
      private const int DEFAULT_MESSAGE_TEXT_LINES = 2;
      private const int MAX_MESSAGE_TEXT_LINES = 8;

      public string? MessageSearchText { get; set; }
      public ConversationMetadata? SelectedConversation { get; set; }
      public string? Message { get; set; }
      public ChatMessage? SelectedMessage { get; set; } = default!;
      public FilteredObservableCollection<ChatMessage> Messages { get; private set; } = default!;

      public int MessageTextLines { get; set; } = DEFAULT_MESSAGE_TEXT_LINES;

      /// <summary>
      /// This is used to prevent the user from sending multiple messages before the first one has been replied to.
      /// When the user send a message, this is set to true, and when the message is replied to, it is set to false.
      /// </summary>
      public bool IsWaitingReply { get; set; }

      public bool CanSendMessage => !IsWaitingReply && SelectedConversation != null;

      public bool HasMessageGenerationFailed { get; internal set; }

      /// <summary>
      /// The response from the chat API, streamed as it comes in.
      /// During the streaming, this is used to store the response, and when the streaming is done, it is 
      /// cleared and the response is added to the <see cref="Messages"/> collection.
      /// </summary>
      public ChatMessage? StreamedResponse { get; set; }

      public void Initialize(Func<IEnumerable<ChatMessage>, ValueTask<IEnumerable<ChatMessage>>> messageFilter)
      {
         MessageSearchText = null;
         Messages = new(messageFilter);
      }

      /// <summary>
      /// Sets the height of the text area to the height of the text (capped at max X lines).
      /// </summary>
      public void SetMessageTextLines()
      {
         var lines = Message?.Split('\n').Length ?? DEFAULT_MESSAGE_TEXT_LINES;

         if (lines < DEFAULT_MESSAGE_TEXT_LINES)
         {
            MessageTextLines = DEFAULT_MESSAGE_TEXT_LINES;
         }
         else if (lines > MAX_MESSAGE_TEXT_LINES)
         {
            MessageTextLines = MAX_MESSAGE_TEXT_LINES;
         }
         else
         {
            MessageTextLines = lines;
         }
      }
   }

   private readonly State _state = new State();
}

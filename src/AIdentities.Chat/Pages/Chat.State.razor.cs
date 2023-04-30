using AIdentities.Chat.Models;
using AIdentities.Shared.Collections;

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

      public FilteredObservableCollection<ChatMessage> Messages { get; private set; } = default!;

      public int MessageTextLines { get; set; } = DEFAULT_MESSAGE_TEXT_LINES;

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

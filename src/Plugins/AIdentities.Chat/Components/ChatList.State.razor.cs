namespace AIdentities.Chat.Components;

public partial class ChatList
{
   class State
   {
      public string? ConversationSearchText { get; set; }
      public FilteredObservableCollection<ChatMetadata> Conversations { get; private set; } = default!;
      public ChatMetadata? SelectedConversation { get; set; }
      public bool IsEditingConversation { get; set; } = false;
      public string EditingTitle { get; set; } = string.Empty;

      public void Initialize(Func<IEnumerable<ChatMetadata>, ValueTask<IEnumerable<ChatMetadata>>> conversationFilter)
      {
         ConversationSearchText = null;
         Conversations = new(conversationFilter);
      }
   }

   private readonly State _state = new();
}

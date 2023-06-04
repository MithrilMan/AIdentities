namespace AIdentities.Chat.Components;

public partial class ConversationList
{
   class State
   {
      public string? ConversationSearchText { get; set; }
      public FilteredObservableCollection<Conversation> Conversations { get; private set; } = default!;
      public Conversation? SelectedConversation { get; set; }
      public bool IsEditingConversation { get; set; } = false;
      public string EditingTitle { get; set; } = string.Empty;

      public void Initialize(Func<IEnumerable<Conversation>, ValueTask<IEnumerable<Conversation>>> conversationFilter)
      {
         ConversationSearchText = null;
         Conversations = new(conversationFilter);
      }
   }

   private readonly State _state = new();
}

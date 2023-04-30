namespace AIdentities.Chat.Components;

public partial class ConversationList
{
   class State
   {
      public string? ConversationSearchText { get; set; }
      public FilteredObservableCollection<ConversationMetadata> Conversations { get; private set; } = default!;
      public ConversationMetadata? SelectedConversation { get; set; }
      public bool IsEditingConversation { get; set; } = false;
      public string EditingTitle { get; set; } = string.Empty;

      public void Initialize(Func<IEnumerable<ConversationMetadata>, ValueTask<IEnumerable<ConversationMetadata>>> conversationFilter)
      {
         ConversationSearchText = null;
         Conversations = new(conversationFilter);
      }
   }

   private readonly State _state = new();
}

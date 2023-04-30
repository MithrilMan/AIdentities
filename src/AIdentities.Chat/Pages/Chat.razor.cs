using AIdentities.Chat.Extendability;
using MudBlazor;

namespace AIdentities.Chat.Pages;

[PageDefinition("Chat", Icons.Material.Filled.ChatBubble, "chat", Description = "Allow the user to chat with one or multiple other AIdentities")]
public partial class Chat : AppPage<Chat>
{
   [Inject] private IChatConnector ChatConnector { get; set; } = null!;
   [Inject] private IChatStorage ChatStorage { get; set; } = null!;

   protected override void OnInitialized()
   {
      base.OnInitialized();
      _state.Initialize(DutyFilter);
   }

   public async ValueTask<IEnumerable<ChatMessage>> DutyFilter(IEnumerable<ChatMessage> unfilteredItems)
   {
      if (_state.MessageSearchText is null) return unfilteredItems;

      await ValueTask.CompletedTask.ConfigureAwait(false);

      unfilteredItems = unfilteredItems
         .Where(duty => duty.Message?.Contains(_state.MessageSearchText, StringComparison.OrdinalIgnoreCase) ?? false);

      return unfilteredItems;
   }

   private Task ApplyFilter() => _state.Messages.ApplyFilterAsync().AsTask();

   private async Task SubmitAsync()
   {
      if (_state.SelectedConversation is null)
      {
         NotificationService.ShowWarning("Please select a conversation first");
         return;
      }

      if (!string.IsNullOrEmpty(_state.Message))
      {
         var message = new ChatMessage()
         {
            Message = _state.Message,
            IsGenerated = false,
            User = "ME",
            //ToUserId = null,
         };

         await ChatStorage.UpdateConversationAsync(_state.SelectedConversation!, message).ConfigureAwait(false);
         await _state.Messages.AppendItemAsync(message).ConfigureAwait(false);

         _state.Message = string.Empty;
      }
   }


   async Task OnConversationChanged()
   {
      if (_state.SelectedConversation is null)
      {
         await _state.Messages.LoadItemsAsync(null).ConfigureAwait(false);
         return;
      }

      var conversation = await ChatStorage.LoadConversationAsync(_state.SelectedConversation.ConversationId).ConfigureAwait(false);
      await _state.Messages.LoadItemsAsync(conversation.Messages).ConfigureAwait(false);
   } 
}

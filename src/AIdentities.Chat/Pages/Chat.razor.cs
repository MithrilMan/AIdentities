using AIdentities.Chat.Extendability;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace AIdentities.Chat.Pages;

[PageDefinition("Chat", Icons.Material.Filled.ChatBubble, "chat", Description = "Allow the user to chat with one or multiple other AIdentities")]
public partial class Chat : AppPage<Chat>
{
   const string LIST_ID = "message-list-wrapper";
   const string LIST_SELECTOR = $"#{LIST_ID}";

   [Inject] private IChatConnector ChatConnector { get; set; } = null!;
   [Inject] private IChatStorage ChatStorage { get; set; } = null!;
   [Inject] private IScrollService ScrollService { get; set; } = null!;

   MudTextField<string>? _messageTextField = default!;

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

   private async Task SendMessageAsync()
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

         // the append has to be done on the same thread the UI is using to render, to prevent "a collection has been modified" exceptions
         await InvokeAsync(() => _state.Messages.AppendItemAsync(message).AsTask()).ConfigureAwait(false);

         _state.Message = string.Empty;
         _state.SetMessageTextLines();

         await ScrollToEndOfMessageList().ConfigureAwait(false);
      }
   }

   private async Task ScrollToEndOfMessageList()
   {
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);
      await ScrollService.ScrollToBottom(LIST_SELECTOR).ConfigureAwait(false);
   }

   async Task OnConversationChanged()
   {
      if (_state.SelectedConversation is null)
      {
         await _state.Messages.LoadItemsAsync(null).ConfigureAwait(false);
         return;
      }

      Conversation conversation;

      try
      {
         conversation = await ChatStorage.LoadConversationAsync(_state.SelectedConversation.ConversationId).ConfigureAwait(false);
      }
      catch (Exception ex)
      {
         NotificationService.ShowError($"Failed to load conversation: {ex.Message}");
         await _state.Messages.LoadItemsAsync(null).ConfigureAwait(false);
         return;
      }

      await _state.Messages.LoadItemsAsync(conversation.Messages).ConfigureAwait(false);
      await ScrollToEndOfMessageList().ConfigureAwait(false);
   }


   async Task OnKeyDown(KeyboardEventArgs e)
   {
      if (e.Key is "Enter" or "NumppadEnter" && !e.ShiftKey)
      {
         await _messageTextField!.BlurAsync().ConfigureAwait(false);
         await SendMessageAsync().ConfigureAwait(false);
         await _messageTextField!.FocusAsync().ConfigureAwait(false);
         return;
      }
   }
}

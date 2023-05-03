using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace AIdentities.Chat.Components;

public partial class ConversationList : ComponentBase
{
   [Inject] public IChatStorage ChatStorage { get; set; } = default!;
   [Inject] IDialogService DialogService { get; set; } = null!;
   [Inject] INotificationService NotificationService { get; set; } = null!;
   [Inject] IAIdentityProvider AIdentityProvider { get; set; } = null!;

   [Parameter] public string? Class { get; set; }
   [Parameter] public ConversationMetadata? Conversation { get; set; }
   [Parameter] public EventCallback<ConversationMetadata> ConversationChanged { get; set; }

   protected override void OnInitialized()
   {
      base.OnInitialized();
      _state.Initialize(ConversationFilter);
   }

   protected override void OnParametersSet()
   {
      base.OnParametersSet();
      _state.SelectedConversation = Conversation;
   }

   protected override async Task OnInitializedAsync()
   {
      var conversations = await ChatStorage.GetConversationsAsync().ConfigureAwait(false);
      await _state.Conversations.LoadItemsAsync(conversations).ConfigureAwait(false);
      await ApplyFilterAsync().ConfigureAwait(false);
   }

   public ValueTask<IEnumerable<ConversationMetadata>> ConversationFilter(IEnumerable<ConversationMetadata> unfilteredItems)
   {
      if (!string.IsNullOrWhiteSpace(_state.ConversationSearchText))
      {
         unfilteredItems = unfilteredItems
         .Where(c => c.Title?.Contains(_state.ConversationSearchText, StringComparison.OrdinalIgnoreCase) ?? false);
      }

      unfilteredItems = unfilteredItems.OrderByDescending(c => c.UpdatedAt);

      return ValueTask.FromResult(unfilteredItems);
   }

   private Task ApplyFilterAsync() => _state.Conversations.ApplyFilterAsync().AsTask();

   async Task OnSelectConversation(ConversationMetadata conversation)
   {
      if (conversation == _state.SelectedConversation) return;

      _state.SelectedConversation = conversation;
      _state.IsEditingConversation = false;
      await ConversationChanged.InvokeAsync(conversation).ConfigureAwait(false);
   }
   async Task StartNewConversationAsync()
   {
      var dialog = await DialogService.ShowAsync<StartConversationDialog>("Start a new conversation", new DialogOptions()
      {
         CloseButton = true,
         CloseOnEscapeKey = true,
         Position = DialogPosition.Center,
         FullWidth = true,
      }).ConfigureAwait(false);

      var result = await dialog.Result.ConfigureAwait(false);
      if (result.Data is not Conversation conversation) return;

      await ChatStorage.StartConversationAsync(conversation).ConfigureAwait(false);
      await _state.Conversations.AppendItemAsync(conversation.Metadata).ConfigureAwait(false);
      await InvokeAsync(() => OnSelectConversation(conversation.Metadata)).ConfigureAwait(false);
      await ApplyFilterAsync().ConfigureAwait(false);
   }

   void EnableRenameConversation(ConversationMetadata conversation)
   {
      _state.SelectedConversation = conversation; //it seems like this should be unnecessary but mudlist doesn't update otherwise
      _state.IsEditingConversation = true;
      _state.EditingTitle = conversation.Title;
   }

   async Task RenameConversation(ConversationMetadata conversation)
   {
      _state.IsEditingConversation = false;
      _state.SelectedConversation!.Title = _state.EditingTitle;
      await ChatStorage.UpdateConversationAsync(conversation, null).ConfigureAwait(false);
      await ApplyFilterAsync().ConfigureAwait(false);
      await InvokeAsync(() => ConversationChanged.InvokeAsync(conversation)).ConfigureAwait(false);
   }

   async Task DeleteConversation(ConversationMetadata conversation)
   {
      bool deletingCurrentConversation = conversation == _state.SelectedConversation;

      bool? result = await DialogService.ShowMessageBox(
           $"Do you want to REMOVE the conversation {conversation.Title}?",
           "Removing a conversation will remove all messages and files associated with it.",
           yesText: "Remove it!", cancelText: "Cancel").ConfigureAwait(false);

      if (result != true) return;

      await ChatStorage.DeleteConversationAsync(conversation.ConversationId).ConfigureAwait(false);
      await _state.Conversations.RemoveItemAsync(conversation).ConfigureAwait(false);

      if (deletingCurrentConversation)
      {
         _state.SelectedConversation = null;
         _state.IsEditingConversation = false;
         await InvokeAsync(() => ConversationChanged.InvokeAsync(null)).ConfigureAwait(false);
      }
   }

   async Task SaveOnEnterPressed(KeyboardEventArgs keyboardEventArgs)
   {
      switch (keyboardEventArgs.Key)
      {
         case "Escape":
            _state.IsEditingConversation = false;
            return;
         case "Enter":
            await RenameConversation(_state.SelectedConversation!).ConfigureAwait(false);
            break;
      }
   }
}

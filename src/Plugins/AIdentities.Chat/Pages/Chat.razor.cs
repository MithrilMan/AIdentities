﻿using AIdentities.Chat.Extendability;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace AIdentities.Chat.Pages;

[PageDefinition("Chat", Icons.Material.Filled.ChatBubble, "chat", Description = "Allow the user to chat with one or multiple other AIdentities")]
public partial class Chat : AppPage<Chat>
{
   const string LIST_ID = "message-list-wrapper";
   const string LIST_SELECTOR = $"#{LIST_ID}";

   [Inject] private IDialogService DialogService { get; set; } = null!;
   [Inject] private IChatConnector ChatConnector { get; set; } = null!;
   [Inject] private IChatStorage ChatStorage { get; set; } = null!;
   [Inject] private IScrollService ScrollService { get; set; } = null!;
   [Inject] private IChatPromptGenerator ChatPromptGenerator { get; set; } = null!;

   MudTextField<string>? _messageTextField = default!;

   protected override void OnInitialized()
   {
      base.OnInitialized();
      _state.Initialize(Filter);
   }

   public async ValueTask<IEnumerable<ChatMessage>> Filter(IEnumerable<ChatMessage> unfilteredItems)
   {
      if (_state.MessageSearchText is null) return unfilteredItems;

      await ValueTask.CompletedTask.ConfigureAwait(false);

      unfilteredItems = unfilteredItems
         .Where(item => item.Message?.Contains(_state.MessageSearchText, StringComparison.OrdinalIgnoreCase) ?? false);

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
            User = "ME"
         };

         await ChatStorage.UpdateConversationAsync(_state.SelectedConversation!, message).ConfigureAwait(false);

         // the append has to be done on the same thread the UI is using to render, to prevent "a collection has been modified" exceptions
         await InvokeAsync(() => _state.Messages.AppendItemAsync(message).AsTask()).ConfigureAwait(false);

         _state.Message = string.Empty;
         _state.SetMessageTextLines();

         await ScrollToEndOfMessageList().ConfigureAwait(false);

         ChatPromptGenerator.AppendMessage(message);
         await SendMessageToConnector().ConfigureAwait(false);
      }
   }

   private async Task SendMessageToConnector()
   {
      _state.IsWaitingReply = true;
      await ScrollToEndOfMessageList().ConfigureAwait(false);
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);

      try
      {
         var request = await ChatPromptGenerator.GenerateApiRequest().ConfigureAwait(false);

         var reply = await ChatConnector.SendMessageAsync(request).ConfigureAwait(false);

         if (reply is not null)
         {
            _state.HasMessageGenerationFailed = false;

            var repliedMessage = new ChatMessage()
            {
               Message = reply.GeneratedMessage,
               IsGenerated = true,
               AIDentityId = _state.SelectedConversation?.AIdentityId
            };
            await ChatStorage.UpdateConversationAsync(_state.SelectedConversation!, repliedMessage).ConfigureAwait(false);
            await InvokeAsync(() => _state.Messages.AppendItemAsync(repliedMessage).AsTask()).ConfigureAwait(false);
            await ScrollToEndOfMessageList().ConfigureAwait(false);
         }
      }
      catch (Exception ex)
      {
         NotificationService.ShowError($"Failed to send message to connector: {ex.Message}");
         _state.HasMessageGenerationFailed = true;
         return;
      }
      finally
      {
         _state.IsWaitingReply = false;
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
         ChatPromptGenerator.InitializeConversation(null);
         await _state.Messages.LoadItemsAsync(null).ConfigureAwait(false);
         return;
      }

      Conversation conversation;

      try
      {
         conversation = await ChatStorage.LoadConversationAsync(_state.SelectedConversation.ConversationId).ConfigureAwait(false);
         ChatPromptGenerator.InitializeConversation(conversation);
         if (conversation.Messages?.LastOrDefault()?.IsGenerated == false)
         {
            // if the last message is not generated, we need to generate a reply so we enable the "resend" button
            _state.HasMessageGenerationFailed = true;
         }
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
         // remove the key from the input field
         _state.Message = _state.Message![..^1];

         //send the message only if we are not waiting for a reply
         if (!_state.IsWaitingReply)
         {
            await SendMessageAsync().ConfigureAwait(false);
         }

         await _messageTextField!.FocusAsync().ConfigureAwait(false);
         return;
      }
   }

   Task Resend() => SendMessageToConnector();

   async Task OnDeleteMessage(ChatMessage message)
   {
      bool? result = await DialogService.ShowMessageBox(
         "Do you want to REMOVE the message?",
         "Removing the message is permanent and cannot be undone.",
         yesText: "Remove it!", cancelText: "Cancel").ConfigureAwait(false);

      if (result != true) return;

      if (await ChatStorage.DeleteMessageAsync(_state.SelectedConversation!, message).ConfigureAwait(false))
      {
         await _state.Messages.RemoveItemAsync(message).ConfigureAwait(false);

         // we cannot remove easily a message from the PromptGenerator, so we just reset the conversation
         var conversation = await ChatStorage.LoadConversationAsync(_state.SelectedConversation!.ConversationId).ConfigureAwait(false);
         ChatPromptGenerator.InitializeConversation(conversation);
         if (conversation.Messages?.LastOrDefault()?.IsGenerated == false)
         {
            // if the last message is not generated, we need to generate a reply so we enable the "resend" button
            _state.HasMessageGenerationFailed = true;
         }
      }
   }
}

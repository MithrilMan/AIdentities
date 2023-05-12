﻿using AIdentities.Shared.Plugins.Connectors.Conversational;
using Microsoft.AspNetCore.Components.Web;
using Toolbelt.Blazor.HotKeys2;

namespace AIdentities.Chat.Pages;

[Route(PAGE_URL)]
[PageDefinition(PAGE_TITLE, Icons.Material.Filled.ChatBubble, PAGE_URL, Description = "Allow the user to chat with one or multiple other AIdentities")]
public partial class Chat : AppPage<Chat>
{
   const string PAGE_TITLE = "Chat";
   const string PAGE_URL = "/chat";
   const string LIST_ID = "message-list-wrapper";
   const string LIST_SELECTOR = $"#{LIST_ID}";

   [Inject] private IDialogService DialogService { get; set; } = null!;
   [Inject] private IEnumerable<IConversationalConnector> ChatConnectors { get; set; } = null!;
   [Inject] private IChatStorage ChatStorage { get; set; } = null!;
   [Inject] private IScrollService ScrollService { get; set; } = null!;
   [Inject] private IChatPromptGenerator ChatPromptGenerator { get; set; } = null!;
   [Inject] private IAIdentityProvider AIdentityProvider { get; set; } = null!;
   [Inject] private IConversationExporter ConversationExporter { get; set; } = null!;


   MudTextFieldExtended<string?> _messageTextField = default!;

   protected override void OnInitialized()
   {
      base.OnInitialized();
      _state.Initialize(Filter);
      _state.Connector = ChatConnectors.FirstOrDefault();
   }

   protected override void ConfigureHotKeys(HotKeysContext hotKeysContext)
   {
      base.ConfigureHotKeys(hotKeysContext);
      hotKeysContext.Add(ModCode.Ctrl, Code.E, OnHotkeyExportConversation, "Exit from the conversation.");
      hotKeysContext.Add(ModCode.None, Code.Delete, OnHotkeyDeleteMessage, "Delete the selected message.");
   }

   private async ValueTask OnHotkeyExportConversation()
   {
      if (_state.SelectedConversation == null) return;

      if (_state.Messages.UnfilteredCount is not > 0)
      {
         NotificationService.ShowWarning("The conversation is empty, nothing to export.");
         return;
      }

      bool? result = await DialogService.ShowMessageBox(
          "Do you want to export the conversation?",
          "Accept to export the conversation and save it locally",
          yesText: "Export it!", cancelText: "Cancel").ConfigureAwait(false);
      if (result != true) return;

      Guid conversationId = _state.SelectedConversation.ConversationId;
      await ConversationExporter.ExportConversationAsync(conversationId, ConversationExportFormat.Html).ConfigureAwait(false);
   }

   private async ValueTask OnHotkeyDeleteMessage()
   {
      if (_state.SelectedMessage == null) return;

      await OnDeleteMessage(_state.SelectedMessage).ConfigureAwait(false);
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);
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

         await ScrollToEndOfMessageList().ConfigureAwait(false);

         ChatPromptGenerator.AppendMessage(message);
         await SendMessageToConnector().ConfigureAwait(false);
      }
   }

   private async Task SendMessageToConnector()
   {
      if (_state.Connector is null)
      {
         NotificationService.ShowError("No chat connector found");
         return;
      }

      _state.IsWaitingReply = true;
      await ScrollToEndOfMessageList().ConfigureAwait(false);
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);

      try
      {
         var request = await ChatPromptGenerator.GenerateApiRequest().ConfigureAwait(false);

         _state.StreamedResponse = new ChatMessage()
         {
            IsGenerated = true,
            AIDentityId = _state.SelectedConversation?.AIdentityId
         };

         var completions = _state.Connector.RequestChatCompletionAsStreamAsync(request, CancellationToken.None)
            .WithCancellation(CancellationToken.None)
            .ConfigureAwait(false);
         await foreach (var completion in completions)
         {
            _state.StreamedResponse.Message += completion.GeneratedMessage;
            // we force the update to show the streamed response
            await ScrollToEndOfMessageList().ConfigureAwait(false);
         }

         if (_state.StreamedResponse.Message?.Length > 0)
         {
            _state.HasMessageGenerationFailed = false;
            ChatPromptGenerator.AppendMessage(_state.StreamedResponse);
            await InvokeAsync(() => _state.Messages.AppendItemAsync(_state.StreamedResponse).AsTask()).ConfigureAwait(false);
            await ScrollToEndOfMessageList().ConfigureAwait(false);
            await ChatStorage.UpdateConversationAsync(_state.SelectedConversation!, _state.StreamedResponse).ConfigureAwait(false);
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
         _state.StreamedResponse = null;
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

         // if the last message is not generated, we need to generate a reply so we enable the "resend" button
         _state.HasMessageGenerationFailed = conversation.Messages?.LastOrDefault()?.IsGenerated == false;
      }
   }

   async Task CloseConversation()
   {
      _state.SelectedConversation = null;
      ChatPromptGenerator.InitializeConversation(null);
      await _state.Messages.LoadItemsAsync(Enumerable.Empty<ChatMessage>()).ConfigureAwait(false);
   }
}

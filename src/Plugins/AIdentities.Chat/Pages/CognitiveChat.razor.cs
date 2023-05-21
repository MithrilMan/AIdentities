using AIdentities.Chat.AIdentiy;
using AIdentities.Chat.Missions;
using AIdentities.Shared.Features.CognitiveEngine;
using AIdentities.Shared.Features.CognitiveEngine.Engines.Mithril;
using AIdentities.Shared.Features.CognitiveEngine.Mission;
using AIdentities.Shared.Features.CognitiveEngine.Skills;
using AIdentities.Shared.Features.Core.Services;
using Microsoft.AspNetCore.Components.Web;
using Toolbelt.Blazor.HotKeys2;

namespace AIdentities.Chat.Pages;

[Route(PAGE_URL)]
[PageDefinition(PAGE_TITLE, IconsEx.CHAT_PLUS, PAGE_URL, Description = PAGE_DESCRIPTION)]
public partial class CognitiveChat : AppPage<CognitiveChat>
{
   const string PAGE_TITLE = "Cognitive Chat";
   const string PAGE_URL = "/cognitive-chat";
   const string PAGE_DESCRIPTION = "Chat with AIdentities and humans, let them chat freely and assist anytime there is a skill you know to execute";
   const string LIST_ID = "message-list-wrapper";
   const string LIST_SELECTOR = $"#{LIST_ID}";

   [Inject] private IDialogService DialogService { get; set; } = null!;
   [Inject] private IEnumerable<IConversationalConnector> ChatConnectors { get; set; } = null!;
   [Inject] private IChatStorage ChatStorage { get; set; } = null!;
   [Inject] private IScrollService ScrollService { get; set; } = null!;
   [Inject] private IChatPromptGenerator ChatPromptGenerator { get; set; } = null!;
   [Inject] private IConversationExporter ConversationExporter { get; set; } = null!;
   [Inject] private IPluginSettingsManager PluginSettingsManager { get; set; } = null!;
   [Inject] private IAIdentityProvider AIdentityProvider { get; set; } = null!;
   [Inject] private ICognitiveEngineProvider CognitiveEngineProvider { get; set; } = null!;

   /// <summary>
   /// The mission that will be assigned to the <see cref="_chatKeeper"/> instance.
   /// </summary>
   [Inject] private CognitiveChatMission CognitiveChatMission { get; set; } = null!;

   /// <summary>
   /// reference to the _chatKeeper instance
   /// </summary>
   private readonly ChatKeeper _chatKeeper = new();
   private ICognitiveEngine _chatKeeperCognitiveEngine = default!;
   private MissionToken? _chatMissionToken;

   MudTextFieldExtended<string?> _messageTextField = default!;

   protected override void OnInitialized()
   {
      base.OnInitialized();
      _state.Initialize(Filter, ChatPromptGenerator, AIdentityProvider);

      var settings = PluginSettingsManager.Get<ChatSettings>();
      _state.Connector = ChatConnectors.FirstOrDefault(c => c.Enabled && c.Name == settings.DefaultConnector);

      StartChatKeeperCognitiveEngine(_chatKeeper, CognitiveChatMission);
   }

   private void StartChatKeeperCognitiveEngine(ChatKeeper chatKeeper, CognitiveChatMission mission)
   {
      _chatKeeperCognitiveEngine = CognitiveEngineProvider.CreateCognitiveEngine<MithrilCognitiveEngine>(chatKeeper);
      _chatMissionToken = _chatKeeperCognitiveEngine.StartMission(mission, PageCancellationToken);
   }

   protected override void ConfigureHotKeys(HotKeysContext hotKeysContext)
   {
      base.ConfigureHotKeys(hotKeysContext);
      hotKeysContext.Add(ModCode.Ctrl, Code.E, ExportConversation, "Export the conversation.");
      hotKeysContext.Add(ModCode.None, Code.Delete, OnHotkeyDeleteMessage, "Delete the selected message.");
   }

   private async ValueTask ExportConversation()
   {
      if (_state.NoConversation) return;

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

      Guid conversationId = _state.SelectedConversation!.ConversationId;
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

   private async Task SendMessageAsync()
   {
      if (_state.NoConversation)
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
         //await SendMessageToConnector().ConfigureAwait(false);

         var responses = _chatKeeperCognitiveEngine.HandlePromptAsync(new UserPrompt
         {
            Text = message.Message,
         }, null, _state.MessageGenerationCancellationTokenSource.Token)
         .ConfigureAwait(false);

         await foreach (var response in responses)
         {
            await InvokeAsync(() => _state.Messages.AppendItemAsync(new ChatMessage
            {
               AIDentityId = response.AIdentityId,
               IsGenerated = true,
               Message = response.Content,
            }).AsTask()).ConfigureAwait(false);
            await ScrollToEndOfMessageList().ConfigureAwait(false);
         }
      }
   }

   private async Task SendMessageToConnector()
   {
      //if (_state.Connector is null)
      //{
      //   NotificationService.ShowError("No chat connector found");
      //   return;
      //}

      //_state.IsWaitingReply = true;
      //await ScrollToEndOfMessageList().ConfigureAwait(false);
      //await InvokeAsync(StateHasChanged).ConfigureAwait(false);

      //try
      //{
      //   var request = await _state.ChatPromptGenerator.GenerateApiRequest().ConfigureAwait(false);
      //   _state.StreamedResponse = new ChatMessage()
      //   {
      //      IsGenerated = true,
      //      AIDentityId = _state.SelectedConversation?.AIdentityIds.FirstOrDefault() //TODO handle multiple AIdentities
      //   };
      //   _state.StreamedResponse.Metadata.Add("Request", request);

      //   var completions = _state.Connector.RequestChatCompletionAsStreamAsync(request, _state.MessageGenerationCancellationTokenSource.Token)
      //      .WithCancellation(_state.MessageGenerationCancellationTokenSource.Token)
      //      .ConfigureAwait(false);

      //   await foreach (var completion in completions)
      //   {
      //      _state.StreamedResponse.Message += completion.GeneratedMessage;
      //      // we force the update to show the streamed response
      //      await ScrollToEndOfMessageList().ConfigureAwait(false);
      //   }

      //   if (_state.StreamedResponse.Message?.Length > 0)
      //   {
      //      _state.HasMessageGenerationFailed = false;
      //      ChatPromptGenerator.AppendMessage(_state.StreamedResponse);
      //      await InvokeAsync(() => _state.Messages.AppendItemAsync(_state.StreamedResponse).AsTask()).ConfigureAwait(false);
      //      await ScrollToEndOfMessageList().ConfigureAwait(false);
      //      await ChatStorage.UpdateConversationAsync(_state.SelectedConversation!, _state.StreamedResponse).ConfigureAwait(false);
      //   }
      //}
      //catch (OperationCanceledException)
      //{
      //   NotificationService.ShowInfo("Message generation cancelled");
      //}
      //catch (Exception ex)
      //{
      //   NotificationService.ShowError($"Failed to send message to connector: {ex.Message}");
      //   _state.HasMessageGenerationFailed = true;
      //   return;
      //}
      //finally
      //{
      //   _state.StreamedResponse = null;
      //   _state.IsWaitingReply = false;
      //}
   }

   private async Task ScrollToEndOfMessageList()
   {
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);
      await ScrollService.ScrollToBottom(LIST_SELECTOR).ConfigureAwait(false);
   }

   async Task OnConversationChanged()
   {
      if (_state.NoConversation)
      {
         await _state.CloseConversation().ConfigureAwait(false);
         CognitiveChatMission.ClearConversation();
         return;
      }

      Conversation conversation;

      try
      {
         conversation = await ChatStorage.LoadConversationAsync(_state.SelectedConversation!.ConversationId).ConfigureAwait(false);
      }
      catch (Exception ex)
      {
         NotificationService.ShowError($"Failed to load conversation: {ex.Message}");
         await _state.CloseConversation().ConfigureAwait(false);
         return;
      }

      await _state.InitializeConversation(conversation).ConfigureAwait(false);

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
         await _state.InitializeConversation(conversation, loadMessages: false).ConfigureAwait(false);
      }
   }

   void StopMessageGeneration()
   {
      _state.MessageGenerationCancellationTokenSource.Cancel();
      _state.MessageGenerationCancellationTokenSource = new CancellationTokenSource();
   }

   async Task AddAIdentityToConversation(AIdentity aIdentity)
   {
      if (_state.SelectedConversation is null)
      {
         NotificationService.ShowError("No conversation selected");
         return;
      }
      if (_state.SelectedConversation.AIdentityIds.Contains(aIdentity.Id))
      {
         NotificationService.ShowError("AIdentity already in conversation");
         return;
      }
      _state.SelectedConversation.AIdentityIds.Add(aIdentity.Id);
      _state.PartecipatingAIdentities.Add(aIdentity);
      await ChatStorage.UpdateConversationAsync(_state.SelectedConversation, null).ConfigureAwait(false);
   }


   public override void Dispose()
   {
      base.Dispose();
      _chatMissionToken?.Dispose();
      _state.MessageGenerationCancellationTokenSource?.Dispose();
   }
}

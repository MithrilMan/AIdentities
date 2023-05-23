using AIdentities.Shared.Features.CognitiveEngine.Thoughts;
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
   [Inject] private IPluginResourcePath PluginResourcePath { get; set; } = null!;


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
   readonly Dictionary<Guid, ChatMessage> _unfinisheMessages = new();

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
      _chatKeeperCognitiveEngine = CognitiveEngineProvider.CreateCognitiveEngine(chatKeeper);
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

         await HandlePrompt(message).ConfigureAwait(false);
      }
   }

   private Task HandlePrompt(ChatMessage message)
   {
      return HandleThoughts(_chatKeeperCognitiveEngine.HandlePromptAsync(
         prompt: new UserPrompt(Guid.Empty, message.Message), // TODO handle the user id
         missionContext: null,
         cancellationToken: _state.MessageGenerationCancellationTokenSource.Token
         ));
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
         _state.ChatKeeperThoughts.Clear();
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

      await HandleThoughts(CognitiveChatMission.StartNewConversationAsync(conversation)).ConfigureAwait(false);

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

   Task Resend() => HandlePrompt(_state.Messages.UnfilteredItems.Last(m => m.AIDentityId != _chatKeeper.Id));

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
         await HandleThoughts(CognitiveChatMission.StartNewConversationAsync(conversation)).ConfigureAwait(false);
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


   /// <summary>
   /// this method handle all the thoughts we receive from the cognitive engines
   /// </summary>
   /// <param name="thoughtsProducer"></param>
   /// <returns></returns>
   public async Task HandleThoughts(IAsyncEnumerable<Thought> thoughtsProducer)
   {
      var thoughts = thoughtsProducer.ConfigureAwait(false);
      await foreach (var thought in thoughts)
      {
         // if we receive a streamed thought, we create a new message and we add it to the list
         // subsequent streamed thoughts with same id will just replace the content of the temporary
         // message until we receive a IsStreamComplete that signal that our message is finally complete
         // and we can add it to the list
         if (thought is StreamedThought streamedThought)
         {
            if (!_unfinisheMessages.TryGetValue(thought.ThoughtId, out var message))
            {
               message = new ChatMessage
               {
                  AIDentityId = thought.AIdentityId,
                  IsGenerated = true,
                  Message = "",
               };
               _unfinisheMessages[thought.ThoughtId] = message;

               if (thought.AIdentityId != _chatKeeper.Id)
               {
                  await InvokeAsync(() => _state.Messages.AppendItemAsync(message).AsTask()).ConfigureAwait(false);
               }
               else
               {
                  _state.ChatKeeperThoughts.Add(thought);
               }
            }

            message.Message = streamedThought.Content;
            await ScrollToEndOfMessageList().ConfigureAwait(false);

            if (streamedThought.IsStreamComplete)
            {
               _unfinisheMessages.Remove(thought.ThoughtId);
               await UpdateChatStorageIfNeeded(thought, message).ConfigureAwait(false);
            }
         }
         else
         {
            // if the thought is not streamed, we just add it to the list
            var message = new ChatMessage
            {
               AIDentityId = thought.AIdentityId,
               IsGenerated = true,
               Message = thought.Content,
            };

            if (thought.AIdentityId != _chatKeeper.Id)
            {
               await InvokeAsync(() => _state.Messages.AppendItemAsync(message).AsTask()).ConfigureAwait(false);
               await ScrollToEndOfMessageList().ConfigureAwait(false);
            }
            else
            {
               _state.ChatKeeperThoughts.Add(thought);
            }
            await UpdateChatStorageIfNeeded(thought, message).ConfigureAwait(false);
         }
      }
   }

   public async Task UpdateChatStorageIfNeeded(Thought generatingThought, ChatMessage message)
   {
      if (generatingThought is FinalThought)
      {
         //final thought are saved in the database because are meaningful conversation messages
         await ChatStorage.UpdateConversationAsync(_state.SelectedConversation!, message).ConfigureAwait(false);
      }
   }


   public override void Dispose()
   {
      base.Dispose();
      _chatMissionToken?.Dispose();
      _state.MessageGenerationCancellationTokenSource?.Dispose();
   }
}

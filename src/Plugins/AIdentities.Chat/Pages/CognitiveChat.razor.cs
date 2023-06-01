﻿using AIdentities.Shared.Plugins.Connectors.TextToSpeech;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Toolbelt.Blazor.HotKeys2;

namespace AIdentities.Chat.Pages;

[Route(PAGE_URL)]
[PageDefinition(PAGE_TITLE, IconsEx.CHAT_PLUS, PAGE_URL, Description = PAGE_DESCRIPTION)]
public partial class CognitiveChat : AppPage<CognitiveChat>
{
   const int CHAT_KEEPER_THINKING_ANIMATION_DURATION = 3000;
   const string PAGE_TITLE = "Cognitive Chat";
   const string PAGE_URL = "/cognitive-chat";
   const string PAGE_DESCRIPTION = "Chat with AIdentities and humans, let them chat freely and assist anytime there is a skill you know to execute";
   const string LIST_ID = "message-list-wrapper";
   const string LIST_SELECTOR = $"#{LIST_ID}";

   [Inject] private IDialogService DialogService { get; set; } = null!;
   [Inject] private IEnumerable<IConversationalConnector> ChatConnectors { get; set; } = null!;
   [Inject] private ICognitiveChatStorage ChatStorage { get; set; } = null!;
   [Inject] private IScrollService ScrollService { get; set; } = null!;
   [Inject] private IConversationExporter ConversationExporter { get; set; } = null!;
   [Inject] private IPluginSettingsManager PluginSettingsManager { get; set; } = null!;
   [Inject] private IAIdentityProvider AIdentityProvider { get; set; } = null!;
   [Inject] private IPluginResourcePath PluginResourcePath { get; set; } = null!;
   [Inject] private IEnumerable<ITextToSpeechConnector> TextToSpeechConnectors { get; set; } = null!;
   [Inject] private IPlayAudioStream PlayAudioStream { get; set; } = null!;

   /// <summary>
   /// The mission that will be assigned to the <see cref="_chatKeeper"/> instance.
   /// </summary>
   [Inject] private CognitiveChatMission CognitiveChatMission { get; set; } = null!;

   /// <summary>
   /// reference to the _chatKeeper instance
   /// </summary>
   readonly Dictionary<Guid, ConversationMessage> _unfinisheMessages = new();

   MudTextFieldExtended<string?> _messageTextField = default!;
   private ChatSettings _chatSettings = default!;

   /// <summary>
   /// After a short delay, we stop the chat keeper thinking animation.
   /// </summary>
   private Action _stopChatKeeperThinkingAnimation = default!;

   protected override void OnInitialized()
   {
      base.OnInitialized();
      _state.Initialize(Filter, AIdentityProvider);

      _chatSettings = PluginSettingsManager.Get<ChatSettings>();
      _state.Connector = ChatConnectors.FirstOrDefault(c => c.Enabled && c.Name == _chatSettings.DefaultConnector);
      _state.TextToSpeechConnector = TextToSpeechConnectors.FirstOrDefault(c => c.Enabled && c.Name == _chatSettings.DefaultTextToSpeechConnector);

      _stopChatKeeperThinkingAnimation = DebounceAsync(async () =>
      {
         _state.IsChatKeeperThinking = false;
         await InvokeAsync(StateHasChanged).ConfigureAwait(false);
      }, TimeSpan.FromMilliseconds(CHAT_KEEPER_THINKING_ANIMATION_DURATION));

      CognitiveChatMission.Start(PageCancellationToken);
      _ = StartConsumingThoughts(PageCancellationToken);
   }

   private async Task StartConsumingThoughts(CancellationToken cancellationToken)
   {
      try
      {
         await foreach (var thought in CognitiveChatMission.Thoughts.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
         {
            await HandleIncomingThought(thought).ConfigureAwait(false);
         }
      }
      catch (Exception ex) when (ex is not OperationCanceledException)
      {
         NotificationService.ShowError(ex.Message);
      }
   }

   private async Task HandleIncomingThought(Thought thought)
   {
      //consider only thoughts that are final or that are from the chat keeper
      if (!thought.IsFinalThought() && thought.AIdentityId != CognitiveChatMission.ChatKeeper.Id) return;

      // if we receive a streamed thought, we create a new message and we add it to the list
      // subsequent streamed thoughts with same id will just replace the content of the temporary
      // message until we receive a IsStreamComplete that signal that our message is finally complete
      // and we can add it to the list
      if (thought.IsStreamedThought(out var streamedThought))
      {
         if (!_unfinisheMessages.TryGetValue(streamedThought.Id, out var message))
         {
            message = new ConversationMessage(
               text: "",
               aIdentity: AIdentityProvider.Get(thought.AIdentityId)!
            );
            _unfinisheMessages[streamedThought.Id] = message;

            if (thought.AIdentityId != CognitiveChatMission.ChatKeeper.Id)
            {
               await InvokeAsync(() => _state.Messages.AppendItemAsync(message).AsTask()).ConfigureAwait(false);
            }
            else
            {
               _state.ChatKeeperThoughts.Add(thought);
            }
         }

         message.UpdateText(streamedThought.Content);
         await ScrollToEndOfMessageList().ConfigureAwait(false);

         if (streamedThought.IsStreamComplete)
         {
            _unfinisheMessages.Remove(streamedThought.Id);
            await UpdateChatStorageIfNeeded(thought, message).ConfigureAwait(false);
         }
      }
      else
      {
         if (thought.AIdentityId != CognitiveChatMission.ChatKeeper.Id)
         {
            // if the thought is not streamed, we just add it to the list
            var message = new ConversationMessage(
               text: thought.Content,
               aIdentity: AIdentityProvider.Get(thought.AIdentityId)!
               );

            await InvokeAsync(() => _state.Messages.AppendItemAsync(message).AsTask()).ConfigureAwait(false);
            await ScrollToEndOfMessageList().ConfigureAwait(false);
            await UpdateChatStorageIfNeeded(thought, message).ConfigureAwait(false);
         }
         else
         {
            await ChatKeeperIsThinking().ConfigureAwait(false);
            _state.ChatKeeperThoughts.Add(thought);
         }
      }
   }

   async Task ChatKeeperIsThinking()
   {
      _state.IsChatKeeperThinking = true;
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);

      _stopChatKeeperThinkingAnimation();
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

      Guid conversationId = _state.SelectedConversation!.Id;
      await ConversationExporter.ExportConversationAsync(conversationId, ConversationExportFormat.Html).ConfigureAwait(false);
   }

   private async ValueTask OnHotkeyDeleteMessage()
   {
      if (_state.SelectedMessage == null) return;

      await OnDeleteMessage(_state.SelectedMessage).ConfigureAwait(false);
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);
   }

   public async ValueTask<IEnumerable<ConversationMessage>> Filter(IEnumerable<ConversationMessage> unfilteredItems)
   {
      if (_state.MessageSearchText is null) return unfilteredItems;

      await ValueTask.CompletedTask.ConfigureAwait(false);

      unfilteredItems = unfilteredItems
         .Where(item => item.Text?.Contains(_state.MessageSearchText, StringComparison.OrdinalIgnoreCase) ?? false);

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
         var message = new ConversationMessage(
            text: _state.Message,
            humanId: Guid.Empty, // TODO handle the user
            humanName: "ME"
            );

         try
         {
            await ChatStorage.UpdateConversationAsync(_state.SelectedConversation!, message).ConfigureAwait(false);
         }
         catch (Exception ex)
         {
            NotificationService.ShowError($"Error while updating the conversation: {ex.Message}");
         }

         // the append has to be done on the same thread the UI is using to render, to prevent "a collection has been modified" exceptions
         await InvokeAsync(() => _state.Messages.AppendItemAsync(message).AsTask()).ConfigureAwait(false);

         _state.Message = string.Empty;

         await ScrollToEndOfMessageList().ConfigureAwait(false);

         await HandlePrompt(message).ConfigureAwait(false);
      }
   }

   private async Task HandlePrompt(ConversationMessage message)
   {
      _state.IsWaitingReply = true;
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);

      try
      {
         await HandleThoughts(CognitiveChatMission.TalkToMissionRunnerAsync(
            prompt: new UserPrompt(Guid.Empty, message.Text ?? ""), // TODO handle the user id
            cancellationToken: _state.MessageGenerationCancellationTokenSource.Token
            )).ConfigureAwait(false);
      }
      catch (Exception ex)
      {
         NotificationService.ShowError($"An error occurred while generating the message: {ex.Message}");
      }

      _state.IsWaitingReply = false;
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);
   }

   private async Task ScrollToEndOfMessageList()
   {
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);
      await Task.Delay(100).ConfigureAwait(false);
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
         conversation = await ChatStorage.LoadConversationAsync(_state.SelectedConversation!.Id).ConfigureAwait(false);
      }
      catch (Exception ex)
      {
         NotificationService.ShowError($"Failed to load conversation: {ex.Message}");
         await _state.CloseConversation().ConfigureAwait(false);
         return;
      }

      await _state.InitializeConversation(conversation).ConfigureAwait(false);

      await CognitiveChatMission.StartNewConversationAsync(conversation).ConfigureAwait(false);

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

   Task Resend() => HandlePrompt(_state.Messages.UnfilteredItems.Last(m => !m.IsAIGenerated));

   async Task OnDeleteMessage(ConversationMessage message)
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
         var conversation = await ChatStorage.LoadConversationAsync(_state.SelectedConversation!.Id).ConfigureAwait(false);
         await _state.InitializeConversation(conversation, loadMessages: false).ConfigureAwait(false);
         await CognitiveChatMission.StartNewConversationAsync(conversation).ConfigureAwait(false);
      }
   }

   void StopMessageGeneration()
   {
      _state.MessageGenerationCancellationTokenSource.Cancel();
      _state.IsWaitingReply = false;
      _state.MessageGenerationCancellationTokenSource = new CancellationTokenSource();
   }

   async Task AddParticipant(AIdentity aIdentity)
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
      _state.SelectedConversation.AddAIdentity(aIdentity);
      CognitiveChatMission.AddParticipant(aIdentity.Id);
      _state.ParticipatingAIdentities.Add(aIdentity);
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
         await HandleIncomingThought(thought).ConfigureAwait(false);
      }
   }

   public async Task UpdateChatStorageIfNeeded(Thought generatingThought, ConversationMessage message)
   {
      if (generatingThought.IsFinalThought())
      {
         //final thought are saved in the database because are meaningful conversation messages
         await ChatStorage.UpdateConversationAsync(_state.SelectedConversation!, message).ConfigureAwait(false);

         if (!_chatSettings.EnableTextToSpeech || _state.TextToSpeechConnector is null) return;

         await PlayAudio(AIdentityProvider.Get(generatingThought.AIdentityId), message).ConfigureAwait(false);
      }
   }

   private async Task PlayAudio(AIdentity? aIdentity, ConversationMessage message)
   {
      if (_state.TextToSpeechConnector is null)
      {
         NotificationService.ShowError("TextToSpeechConnector not available");
         return;
      }

      try
      {
         await _state.TextToSpeechConnector.RequestTextToSpeechAsStreamAsync(
               new DefaultTextToSpeechRequest(aIdentity, message.Text ?? ""),
               async (stream) =>
               {
                  using var streamRef = new DotNetStreamReference(stream: stream);
                  await PlayAudioStream.PlayAudioFileStream(streamRef).ConfigureAwait(false);
               },
               PageCancellationToken
               ).ConfigureAwait(false);
      }
      catch (Exception ex)
      {
         NotificationService.ShowError($"Error while playing audio: {ex.Message}");
      }
   }

   void OnIsModeratorModeEnabledChanged() => CognitiveChatMission.SetModeratedMode(_state.IsModeratorModeEnabled);
   public void SetNextTalker(AIdentity aIdentity) => CognitiveChatMission.SetNextTalker(aIdentity);
   Task ReplyToLastMessage(AIdentity talker)
   {
      SetNextTalker(talker);
      return CognitiveChatMission.ReplyToMessageAsync(_state.SelectedConversation?.Messages.LastOrDefault());
   }

   Task ReplyToSelectedMessage(AIdentity talker)
   {
      SetNextTalker(talker);
      return CognitiveChatMission.ReplyToMessageAsync(_state.SelectedMessage);
   }

   public override void Dispose()
   {
      base.Dispose();
      CognitiveChatMission?.Dispose();
      _state.MessageGenerationCancellationTokenSource?.Dispose();
   }
}

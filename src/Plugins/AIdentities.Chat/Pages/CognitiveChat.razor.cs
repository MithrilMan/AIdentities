using System.Collections.Concurrent;
using AIdentities.Shared.Features.Core.SpeechRecognition;
using AIdentities.Shared.Plugins.Connectors.TextToSpeech;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Toolbelt.Blazor.HotKeys2;

namespace AIdentities.Chat.Pages;

[Route(PAGE_URL)]
[PageDefinition(PAGE_TITLE, IconsEx.CHAT_PLUS, PAGE_URL, Description = PAGE_DESCRIPTION)]
public partial class CognitiveChat : AppPage<CognitiveChat>, ISpeechRecognitionListener
{
   const int CHAT_KEEPER_THINKING_ANIMATION_DURATION = 3000;
   const string PAGE_TITLE = "Cognitive Chat";
   const string PAGE_URL = "/cognitive-chat";
   const string PAGE_DESCRIPTION = "Chat with AIdentities and humans, let them chat freely and assist anytime there is a skill you know to execute";
   const string LIST_ID = "message-list-wrapper";
   const string LIST_SELECTOR = $"#{LIST_ID}";

   [Inject] private IEnumerable<IConversationalConnector> ChatConnectors { get; set; } = null!;
   [Inject] private ICognitiveChatStorage ChatStorage { get; set; } = null!;
   [Inject] private IScrollService ScrollService { get; set; } = null!;
   [Inject] private IConversationExporter ConversationExporter { get; set; } = null!;
   [Inject] private IPluginSettingsManager PluginSettingsManager { get; set; } = null!;
   [Inject] private IAIdentityProvider AIdentityProvider { get; set; } = null!;
   [Inject] private IPluginResourcePath PluginResourcePath { get; set; } = null!;
   [Inject] private IEnumerable<ITextToSpeechConnector> TextToSpeechConnectors { get; set; } = null!;
   [Inject] private IPlayAudioStream PlayAudioStream { get; set; } = null!;
   [Inject] private ISpeechRecognitionService SpeechRecognitionService { get; set; } = null!;

   /// <summary>
   /// The mission that will be assigned to the <see cref="_chatKeeper"/> instance.
   /// </summary>
   [Inject] private CognitiveChatMission CognitiveChatMission { get; set; } = null!;

   /// <summary>
   /// reference to the _chatKeeper instance
   /// </summary>
   //readonly Dictionary<Guid, ConversationMessage> _unfinisheMessages = new();
   readonly ConcurrentDictionary<Guid, IStreamedThought> _streamingChatKeeperThoughts = new();
   readonly ConcurrentDictionary<Guid, ExtendedConversationMessage> _streamingMessages = new();

   MudTextFieldExtended<string?> _messageTextField = default!;
   private ChatSettings _chatSettings = default!;

   /// <summary>
   /// After a short delay, we stop the chat keeper thinking animation.
   /// </summary>
   private Action _stopChatKeeperThinkingAnimation = default!;

   protected override void OnInitialized()
   {
      base.OnInitialized();
      _state.Initialize(CognitiveChatMission, AIdentityProvider, PlayAudioStream);

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

      if (_chatSettings.EnableSpeechRecognition)
      {
         SpeechRecognitionService.InitializeSpeechRecognitionAsync(this);
      }
   }


   protected override void ConfigureHotKeys(HotKeysContext hotKeysContext)
   {
      base.ConfigureHotKeys(hotKeysContext);
      hotKeysContext.Add(ModCode.Ctrl, Code.E, ExportConversation, "Export the conversation.");
      hotKeysContext.Add(ModCode.None, Code.Delete, OnHotkeyDeleteMessage, "Delete the selected message.");
      hotKeysContext.Add(ModCode.Ctrl, Code.M, OnHotkeyModerate, "Toggle the conversation moderation mode, enabling custom tools to chose who can talk and about what.");
      hotKeysContext.Add(ModCode.None, Code.ArrowUp, OnHotkeyArrowUp, "Select the previous message");
      hotKeysContext.Add(ModCode.None, Code.ArrowDown, OnHotkeyArrowDown, "Select the next message");
      hotKeysContext.Add(ModCode.Ctrl, Code.R, OnHotkeyResend, "Resend last message");
      hotKeysContext.Add(ModCode.Ctrl, Code.Space, OnHotkeyVoiceRecognition, "Resend last message");
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
         if (thought.AIdentityId == CognitiveChatMission.ChatKeeper.Id)
         {
            if (!_streamingChatKeeperThoughts.TryGetValue(streamedThought.Id, out var streamingThought))
            {
               _streamingChatKeeperThoughts[streamedThought.Id] = streamedThought;
            }

            if (streamedThought.IsStreamComplete)
            {
               _streamingChatKeeperThoughts.TryRemove(streamedThought.Id, out _);
               _state.ChatKeeperThoughts.Add(thought);
            }
         }
         else
         {
            if (!_streamingMessages.TryGetValue(streamedThought.Id, out var streamingMessage))
            {
               streamingMessage = new ExtendedConversationMessage(
                  text: "",
                  aIdentity: AIdentityProvider.Get(thought.AIdentityId)!,
                  isGeneratingSpeech: false,
                  isComplete: false
                  );

               _streamingMessages[streamedThought.Id] = streamingMessage;
            }

            streamingMessage.UpdateText(streamedThought.Content);
            await ScrollToEndOfMessageList().ConfigureAwait(false);

            if (streamedThought.IsStreamComplete)
            {
               streamingMessage.IsComplete = true;
               _streamingMessages.Remove(streamedThought.Id, out _);
               await UpdateChatStorageIfNeeded(thought, streamingMessage).ConfigureAwait(false);
            }
         }
      }
      else
      {
         if (thought.AIdentityId == CognitiveChatMission.ChatKeeper.Id)
         {
            await ChatKeeperIsThinking().ConfigureAwait(false);
            _state.ChatKeeperThoughts.Add(thought);
         }
         else
         {
            // if the thought is not streamed, we just add it to the list
            var message = new ExtendedConversationMessage(
               text: thought.Content,
               aIdentity: AIdentityProvider.Get(thought.AIdentityId)!,
                isGeneratingSpeech: false,
                isComplete: true
               );

            await ScrollToEndOfMessageList().ConfigureAwait(false);
            await UpdateChatStorageIfNeeded(thought, message).ConfigureAwait(false);
         }
      }
   }

   async Task ChatKeeperIsThinking()
   {
      _state.IsChatKeeperThinking = true;
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);

      _stopChatKeeperThinkingAnimation();
   }

   private async ValueTask ExportConversation()
   {
      if (_state.NoConversation) return;

      if (_state.CurrentConversation is not { Messages.Count: > 0 })
      {
         NotificationService.ShowWarning("The conversation is empty, nothing to export.");
         return;
      }

      bool? result = await DialogService.ShowMessageBox(
          "Do you want to export the conversation?",
          "Accept to export the conversation and save it locally",
          yesText: "Export it!", cancelText: "Cancel").ConfigureAwait(false);
      if (result != true) return;

      Guid conversationId = _state.CurrentConversation.Id;
      await ConversationExporter.ExportConversationAsync(conversationId, ConversationExportFormat.Html).ConfigureAwait(false);
   }

   private async ValueTask ClearConversation()
   {
      if (_state.NoConversation) return;

      if (_state.CurrentConversation is not { Messages.Count: > 0 })
      {
         NotificationService.ShowWarning("The conversation is empty, nothing to remove.");
         return;
      }

      bool? result = await DialogService.ShowMessageBox(
          "Do you want to completely clear the conversation?",
          "Accept to clear the conversation and remove all messages. Current participants will remain in the conversation.",
          yesText: "Yes, clear it!", cancelText: "Cancel").ConfigureAwait(false);
      if (result != true) return;

      await ChatStorage.ClearConversation(_state.CurrentConversation).ConfigureAwait(false);
   }

   private async ValueTask OnHotkeyDeleteMessage()
   {
      if (_state.SelectedMessage == null) return;

      await OnDeleteMessage(_state.SelectedMessage).ConfigureAwait(false);
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);
   }

   private async ValueTask OnHotkeyModerate()
   {
      if (_state.NoConversation) return;

      _state.IsModeratorModeEnabled = !_state.IsModeratorModeEnabled;
      OnIsModeratorModeEnabledChanged();
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);
   }

   private async ValueTask OnHotkeyArrowUp()
   {
      if (_state.NoConversation || _state.SelectedMessage is null) return;

      var previousMessage = _state.CurrentConversation.Messages.TakeWhile(m => m != _state.SelectedMessage).LastOrDefault();
      if (previousMessage is not null)
      {
         _state.SelectedMessage = previousMessage;
      }

      await InvokeAsync(StateHasChanged).ConfigureAwait(false);
      await ScrollService.EnsureIsVisible($"{LIST_SELECTOR} .mud-selected-item").ConfigureAwait(false);
   }

   private async ValueTask OnHotkeyArrowDown()
   {
      if (_state.NoConversation || _state.SelectedMessage is null) return;

      var nextMessage = _state.CurrentConversation.Messages.SkipWhile(m => m != _state.SelectedMessage).Skip(1).FirstOrDefault();
      if (nextMessage is not null)
      {
         _state.SelectedMessage = nextMessage;
      }

      await InvokeAsync(StateHasChanged).ConfigureAwait(false);
      await ScrollService.EnsureIsVisible($"{LIST_SELECTOR} .mud-selected-item").ConfigureAwait(false);
   }

   private async ValueTask OnHotkeyResend()
   {
      if (!_state.HasMessageGenerationFailed) return;

      await Resend().ConfigureAwait(false);
   }

   private async ValueTask OnHotkeyVoiceRecognition()
   {
      if (!_state.CanSendMessage) return;

      try
      {
         if (_state.IsRecognizingVoice)
         {
            await StopVoiceRecognition().ConfigureAwait(false);
         }
         else
         {
            await StartVoiceRecognition().ConfigureAwait(false);
         }
      }
      catch (Exception ex)
      {
         NotificationService.ShowError($"Error while starting voice recognition: {ex.Message}");
      }
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
            await ChatStorage.UpdateConversationAsync(_state.CurrentConversation!, message).ConfigureAwait(false);
         }
         catch (Exception ex)
         {
            NotificationService.ShowError($"Error while updating the conversation: {ex.Message}");
         }

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
      await Task.Delay(250).ConfigureAwait(false);
      await ScrollService.ScrollToBottom(LIST_SELECTOR).ConfigureAwait(false);
   }

   async Task OnConversationChanged(Conversation conversation)
   {
      await PlayAudioStream.StopAudioFiles().ConfigureAwait(false);
      if (conversation == null)
      {
         _state.CloseConversation();
         return;
      }

      try
      {
         conversation = await ChatStorage.LoadConversationAsync(conversation.Id).ConfigureAwait(false);
         await _state.InitializeConversation(conversation).ConfigureAwait(false);
         await ScrollToEndOfMessageList().ConfigureAwait(false);
      }
      catch (Exception ex)
      {
         NotificationService.ShowError($"Failed to load conversation: {ex.Message}");
         _state.CloseConversation();
         return;
      }
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

   Task Resend() => HandlePrompt(_state.CurrentConversation!.Messages.Last(m => !m.IsAIGenerated));

   async Task OnDeleteMessage(ConversationMessage message)
   {
      if (_state.CurrentConversation is null) return;

      bool? result = await DialogService.ShowMessageBox(
         "Do you want to REMOVE the message?",
         "Removing the message is permanent and cannot be undone.",
         yesText: "Remove it!", cancelText: "Cancel").ConfigureAwait(false);

      if (result != true) return;


      // we store the next selectable message, to select it after the deletion
      var nextSelectableMessage = _state.CurrentConversation.Messages.TakeWhile(m => m != message).LastOrDefault()
         ?? _state.CurrentConversation.Messages.Skip(1).FirstOrDefault();

      if (await ChatStorage.DeleteMessageAsync(_state.CurrentConversation, message).ConfigureAwait(false))
      {
         bool deleted = _state.CurrentConversation.RemoveMessage(message.Id);

         if (_state.SelectedMessage == message)
         {
            _state.SelectedMessage = nextSelectableMessage;
         }
      }
   }

   async Task OnPlayAudio(ConversationMessage message)
   {
      if (message.Audio is not { Length: > 0 })
      {
         NotificationService.ShowError("No audio available");
         return;
      }

      using var memoryStream = new MemoryStream(message.Audio);
      using var streamRef = new DotNetStreamReference(memoryStream);
      await PlayAudioStream.StopAudioFiles().ConfigureAwait(false);
      await PlayAudioStream.PlayAudioFileStream(streamRef).ConfigureAwait(false);
   }

   Task OnStopAudio(ConversationMessage message)
   {
      StopPlayingAudio();
      return Task.CompletedTask;
   }

   void StopMessageGeneration()
   {
      _state.MessageGenerationCancellationTokenSource.Cancel();
      _state.IsWaitingReply = false;
      _state.MessageGenerationCancellationTokenSource = new CancellationTokenSource();
   }

   void StopPlayingAudio()
   {
      _state.PlayingSpeechCancellationTokenSource.Cancel();
      _state.IsWaitingReply = false;
      _state.PlayingSpeechCancellationTokenSource = new CancellationTokenSource();
   }

   async Task AddParticipant(AIdentity aIdentity)
   {
      if (_state.NoConversation)
      {
         NotificationService.ShowError("No conversation selected");
         return;
      }

      if (_state.CurrentConversation.AIdentityIds.Contains(aIdentity.Id))
      {
         NotificationService.ShowError("AIdentity already in conversation");
         return;
      }

      await CognitiveChatMission.AddParticipant(aIdentity.Id, true).ConfigureAwait(false);
      await ChatStorage.UpdateConversationAsync(_state.CurrentConversation, null).ConfigureAwait(false);
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

   public async Task UpdateChatStorageIfNeeded(Thought generatingThought, ExtendedConversationMessage message)
   {
      if (_state.NoConversation) return;

      if (generatingThought.IsFinalThought())
      {
         //final thought are saved in the database because are meaningful conversation messages
         await ChatStorage.UpdateConversationAsync(_state.CurrentConversation, message).ConfigureAwait(false);

         if (_state.TextToSpeechConnector is not null
            && _chatSettings is { TextToSpeechMode: TextToSpeechMode.Automatic, EnableTextToSpeech: true })
         {
            await GenerateTextToSpeech(AIdentityProvider.Get(generatingThought.AIdentityId), message).ConfigureAwait(false);
         }
      }
   }

   private async Task GenerateTextToSpeech(AIdentity? aIdentity, ExtendedConversationMessage message)
   {
      if (_state.NoConversation) return;

      if (_state.TextToSpeechConnector is null)
      {
         NotificationService.ShowError("TextToSpeechConnector not available");
         return;
      }

      try
      {
         message.IsGeneratingSpeech = true;
         await InvokeAsync(StateHasChanged).ConfigureAwait(false);
         await _state.TextToSpeechConnector.RequestTextToSpeechAsStreamAsync(
            new DefaultTextToSpeechRequest(aIdentity, message.Text ?? ""),
            async (stream) =>
            {
               // store the streamRef audio generated into the Audio property of the message
               using var memoryStream = new MemoryStream();
               await stream.CopyToAsync(memoryStream).ConfigureAwait(false);
               message.SetAudio(memoryStream.ToArray());
               await ChatStorage.UpdateConversationAsync(_state.CurrentConversation, null).ConfigureAwait(false);

               // play the audio
               stream.Position = 0;
               using var streamRef = new DotNetStreamReference(stream: stream);
               await PlayAudioStream.StopAudioFiles().ConfigureAwait(false);
               await PlayAudioStream.PlayAudioFileStream(streamRef).ConfigureAwait(false);
            },
            PageCancellationToken
            ).ConfigureAwait(false);
      }
      catch (Exception ex)
      {
         NotificationService.ShowError($"Error while playing audio: {ex.Message}");
      }
      finally
      {
         message.IsGeneratingSpeech = false;
         await InvokeAsync(StateHasChanged).ConfigureAwait(false);
      }
   }

   void OnIsModeratorModeEnabledChanged() => CognitiveChatMission.SetModeratedMode(_state.IsModeratorModeEnabled);
   public void SetNextTalker(AIdentity aIdentity) => CognitiveChatMission.SetNextTalker(aIdentity);
   async Task ReplyToLastMessage(AIdentity talker)
   {
      if (_state.NoConversation) return;

      SetNextTalker(talker);
      await CognitiveChatMission.ReplyToMessageAsync(_state.CurrentConversation.Messages.LastOrDefault()).ConfigureAwait(false);
   }

   Task ReplyToSelectedMessage(AIdentity talker)
   {
      SetNextTalker(talker);
      return CognitiveChatMission.ReplyToMessageAsync(_state.SelectedMessage);
   }

   IEnumerable<ConversationMessage> GetMessagesToDisplay()
   {
      if (_state.NoConversation) return Enumerable.Empty<ConversationMessage>();

      return _state.CurrentConversation.Messages
         .Union(_streamingMessages.Values.ToList())
         .OrderBy(m => m.CreationDate);
   }

   async Task StartVoiceRecognition() => await SpeechRecognitionService.StartSpeechRecognitionAsync(
      language: _chatSettings.SpeechRecognitionLanguage ?? "EN-US",
      continuous: true,
      interimResults: true
      ).ConfigureAwait(false);

   async Task StopVoiceRecognition() => await SpeechRecognitionService
         .CancelSpeechRecognitionAsync(false)
         .ConfigureAwait(false);

   #region SpeechRecognition listener
   public async Task OnVoiceRecognized(string transcript, bool isFinal)
   {
      if (_state.NoConversation) return;

      _state.Message = transcript;
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);

      // we stop voice recognition when the user stop talking
      // if continuous speech recognition is disabled
      if (isFinal && !_chatSettings.EnableContinuousSpeechRecognition)
      {
         await StopVoiceRecognition().ConfigureAwait(false);
      }

      //send the message only if we are not waiting for a reply
      if (!_state.IsWaitingReply && isFinal)
      {
         await SendMessageAsync().ConfigureAwait(false);
      }
   }

   public async Task OnVoiceRecognitionStarted()
   {
      _state.IsRecognizingVoice = true;
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);
   }

   public async Task OnVoiceRecognitionFinished()
   {
      _state.IsRecognizingVoice = false;
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);
   }

   public async Task OnVoiceRecognitionError(SpeechRecognitionError error)
   {
      NotificationService.ShowError($"Error while recognizing speech: {error.Error}");
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);
   }
   #endregion

   public override void Dispose()
   {
      base.Dispose();
      CognitiveChatMission?.Dispose();
      _state.PlayingSpeechCancellationTokenSource?.Dispose();
      _state.MessageGenerationCancellationTokenSource?.Dispose();
      _ = PlayAudioStream.StopAudioFiles();
   }
}

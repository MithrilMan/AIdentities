using Microsoft.AspNetCore.Components.Web;
using MudExtensions;

namespace AIdentities.BrainButler.Pages;

[Route(PAGE_URL)]
[PageDefinition(PAGE_TITLE, Icons.Material.Filled.Assistant, PAGE_URL, Description = PAGE_DESCRIPTION)]
public partial class Interaction : AppPage<Interaction>
{
   const string PAGE_TITLE = "BrainButler";
   const string PAGE_URL = "brain-butler";
   const string PAGE_DESCRIPTION = "Allows the user to interact with BrainButler, the personalized assistant.";
   const string LIST_ID = "message-list-wrapper";
   const string LIST_SELECTOR = $"#{LIST_ID}";

   [Inject] private IDialogService DialogService { get; set; } = null!;
   [Inject] private IEnumerable<ICompletionConnector> Connectors { get; set; } = null!;
   [Inject] private IScrollService ScrollService { get; set; } = null!;
   [Inject] private IPromptGenerator PromptGenerator { get; set; } = null!;

   MudTextFieldExtended<string?> _messageTextField = default!;

   protected override void OnInitialized()
   {
      base.OnInitialized();

      //var settings = PluginSettingsManager.Get<ChatSettings>();
      _state.Connector = Connectors.FirstOrDefault(c => c.Enabled); //TODO lasciare che sia configurabile
   }

   private async Task ScrollToEndOfMessageList()
   {
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);
      await ScrollService.ScrollToBottom(LIST_SELECTOR).ConfigureAwait(false);
   }

   Task OnDeleteMessage() => Task.CompletedTask;

   Task Resend() => ReplyToUserRequest(_state.ConversationPieces.LastOrDefault()?.Message);

   async Task OnKeyDown(KeyboardEventArgs e)
   {
      if (e.Key is "Enter" or "NumppadEnter" && !e.ShiftKey)
      {
         await _messageTextField!.BlurAsync().ConfigureAwait(false);
         // remove the key from the input field
         _state.UserRequest = _state.UserRequest![..^1];

         //send the message only if we are not waiting for a reply
         if (!_state.IsWaitingReply)
         {
            await SendMessageAsync().ConfigureAwait(false);
         }

         await _messageTextField!.FocusAsync().ConfigureAwait(false);
         return;
      }
   }

   private async Task SendMessageAsync()
   {
      if (!string.IsNullOrEmpty(_state.UserRequest))
      {
         var message = new UserRequest()
         {
            Message = _state.UserRequest
         };

         // the append has to be done on the same thread the UI is using to render, to prevent "a collection has been modified" exceptions
         _state.ConversationPieces.Add(message);
         _state.UserRequest = string.Empty;

         await ScrollToEndOfMessageList().ConfigureAwait(false);
         await ReplyToUserRequest(message.Message).ConfigureAwait(false);
      }
   }

   void StopMessageGeneration()
   {
      _state.MessageGenerationCancellationTokenSource.Cancel();
      _state.MessageGenerationCancellationTokenSource = new CancellationTokenSource();
   }

   private async Task ReplyToUserRequest(string userRequest)
   {
      if (_state.Connector is null)
      {
         NotificationService.ShowError("No chat connector found");
         return;
      }

      if (string.IsNullOrWhiteSpace(userRequest))
      {
         NotificationService.ShowWarning("Please enter a request to send");
         return;
      }

      _state.IsWaitingReply = true;
      await ScrollToEndOfMessageList().ConfigureAwait(false);

      try
      {
         var prompt = PromptGenerator.GenerateFindCommandPrompt(userRequest);

         _state.IsThinking = true;
         _state.ConversationPieces.Add(new AIResponse
         {
            Message = "Thinking..."
         });
         await ScrollToEndOfMessageList().ConfigureAwait(false);

         var response = await _state.Connector.RequestCompletionAsync(new DefaultCompletionRequest
         {
            Prompt = prompt,
         }).ConfigureAwait(false);

         var detectedCommand = response!.GeneratedMessage;

         if (detectedCommand is null || detectedCommand == "DUNNO")
         {
            _state.ConversationPieces.Add(new AIResponse
            {
               Message = "Sorry, I didn't understand your request"
            });
            await ScrollToEndOfMessageList().ConfigureAwait(false);
            return;
         }

         _state.ConversationPieces.Add(new AIResponse
         {
            Message = $"Ok, so it seems you want me to call {detectedCommand}, let's do it..."
         });
         await ScrollToEndOfMessageList().ConfigureAwait(false);

         prompt = PromptGenerator.GenerateCommandExtraction(detectedCommand, userRequest, out var commandToExecute);
         response = await _state.Connector.RequestCompletionAsync(new DefaultCompletionRequest
         {
            Prompt = prompt,
            MaxGeneratedTokens = 500
         }).ConfigureAwait(false);

         if (response is null || commandToExecute is null)
         {
            _state.ConversationPieces.Add(new AIResponse
            {
               Message = "Sorry, I didn't understand your request"
            });
            await ScrollToEndOfMessageList().ConfigureAwait(false);
            return;
         }

         _state.ConversationPieces.Add(new AIResponse
         {
            Message = $"{response.GeneratedMessage}"
         });
         await ScrollToEndOfMessageList().ConfigureAwait(false);

         var commandResult = await commandToExecute.ExecuteAsync(
            userPrompt: userRequest,
            inputPrompt: response.GeneratedMessage
            ).ConfigureAwait(false);

         _state.ConversationPieces.Add(new AIResponse
         {
            Message = $"{commandResult}"
         });
         await ScrollToEndOfMessageList().ConfigureAwait(false);

         //_state.StreamedResponse = new AIResponse()
         //{
         //   IsGenerated = true,
         //   AIDentityId = _state.SelectedConversation?.AIdentityId
         //};         

         //var completions = _state.Connector.RequestChatCompletionAsStreamAsync(request, _state.MessageGenerationCancellationTokenSource.Token)
         //   .WithCancellation(_state.MessageGenerationCancellationTokenSource.Token)
         //   .ConfigureAwait(false);

         //await foreach (var completion in completions)
         //{
         //   _state.StreamedResponse.Message += completion.GeneratedMessage;
         //   // we force the update to show the streamed response
         //   await ScrollToEndOfMessageList().ConfigureAwait(false);
         //}

         //if (_state.StreamedResponse.Message?.Length > 0)
         //{
         //   _state.HasMessageGenerationFailed = false;
         //   ChatPromptGenerator.AppendMessage(_state.StreamedResponse);
         //   await InvokeAsync(() => _state.Messages.AppendItemAsync(_state.StreamedResponse).AsTask()).ConfigureAwait(false);
         //   await ScrollToEndOfMessageList().ConfigureAwait(false);
         //   await ChatStorage.UpdateConversationAsync(_state.SelectedConversation!, _state.StreamedResponse).ConfigureAwait(false);
         //}
      }
      catch (OperationCanceledException)
      {
         NotificationService.ShowInfo("Message generation cancelled");
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
}

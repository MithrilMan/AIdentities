using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components.Web;
using MudExtensions;

namespace AIdentities.BrainButler.Pages;

[Route(PAGE_URL)]
[PageDefinition(PAGE_TITLE, PAGE_ICON, PAGE_URL, Description = PAGE_DESCRIPTION)]
public partial class Interaction : AppPage<Interaction>
{
   const string PAGE_TITLE = "BrainButler";
   const string PAGE_ICON = Icons.Material.Filled.Assistant;
   const string PAGE_URL = "brain-butler";
   const string PAGE_DESCRIPTION = "Allows the user to interact with BrainButler, the personalized assistant.";
   const string LIST_ID = "message-list-wrapper";
   const string LIST_SELECTOR = $"#{LIST_ID}";

   [Inject] private IConnectorsManager ConnectorsManager { get; set; } = default!;
   [Inject] private IScrollService ScrollService { get; set; } = null!;
   [Inject] private IPromptGenerator PromptGenerator { get; set; } = null!;

   MudTextFieldExtended<string?> _messageTextField = default!;
   AIdentiy.BrainButler _brainButler = new();

   protected override void OnInitialized()
   {
      base.OnInitialized();

      _state.CompletionConnector = ConnectorsManager.GetCompletionConnector();
      _state.ConversationalConnector = ConnectorsManager.GetConversationalConnector();
   }

   private async Task ScrollToEndOfMessageList()
   {
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);
      await ScrollService.ScrollToBottom(LIST_SELECTOR).ConfigureAwait(false);
   }

   Task Resend() => ReplyToUserRequest(_state.ConversationPieces.LastOrDefault()?.Message ?? "");

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

   private async ValueTask AppendAIReply(string? message)
   {
      if (message is null) return;

      _state.ConversationPieces.Add(new AIResponse
      {
         Message = message
      });
      await ScrollToEndOfMessageList().ConfigureAwait(false);
   }

   private async Task ReplyToUserRequest(string userRequest)
   {
      if (_state.CompletionConnector is null)
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

         var response = await _state.CompletionConnector.RequestCompletionAsync(new CompletionRequest(
            AIdentity: _brainButler,
            Prompt: prompt
            )
         {
            MaxGeneratedTokens = 250
         }, _state.MessageGenerationCancellationTokenSource.Token).ConfigureAwait(false);

         var detectedCommand = ExtractCommandName().Match(response!.GeneratedMessage!).Value;

         //var detectedCommand = response!.GeneratedMessage;

         if (detectedCommand is null || detectedCommand == "DUNNO")
         {
            //await AppendAIReply("Sorry, I didn't understand your request").ConfigureAwait(false);
            // if it doesn't understand the request, let's try to reply in conversational style

            _state.StreamedResponse = new AIResponse { };
            var streamedResponse = _state.ConversationalConnector!.RequestChatCompletionAsStreamAsync(new ConversationalRequest(
               AIdentity: _brainButler,
               Messages: new List<ConversationalMessage>()
               {
                   new ConversationalMessage(
                      Role: ConversationalRole.System,
                      Content: "You are Brain Butler, a funny helpful AI agent that loves to make joke when giving back informations.",
                      null
                      ),
                   new ConversationalMessage(
                      Role: ConversationalRole.User,
                      Content: userRequest,
                      null
                      ),
               })
            {
               MaxGeneratedTokens = 500
            }, _state.MessageGenerationCancellationTokenSource.Token).ConfigureAwait(false);

            await foreach (var fragment in streamedResponse)
            {
               _state.StreamedResponse.Message += fragment.GeneratedMessage;
            }

            await AppendAIReply(_state.StreamedResponse.Message).ConfigureAwait(false);
            _state.StreamedResponse = null;

            return;
         }

         await AppendAIReply($"Ok, so it seems you want me to call {detectedCommand}, let's do it...").ConfigureAwait(false);

         prompt = PromptGenerator.GenerateCommandExtraction(detectedCommand, userRequest, out var commandToExecute);
         if (prompt is null || commandToExecute is null)
         {
            await AppendAIReply("Sorry, I didn't understand your request").ConfigureAwait(false);
            return;
         }

         response = await _state.CompletionConnector.RequestCompletionAsync(new CompletionRequest(
            AIdentity: _brainButler,
            Prompt: prompt
            )
         {
            MaxGeneratedTokens = 500
         }, _state.MessageGenerationCancellationTokenSource.Token).ConfigureAwait(false);

         if (response is null || commandToExecute is null)
         {
            await AppendAIReply("Sorry, I didn't understand your request").ConfigureAwait(false);
            return;
         }

         await AppendAIReply(response.GeneratedMessage).ConfigureAwait(false);

         var commandStreamFragments = commandToExecute!.ExecuteAsync(
            userPrompt: userRequest,
            inputPrompt: response.GeneratedMessage
            )
            .WithCancellation(_state.MessageGenerationCancellationTokenSource.Token)
            .ConfigureAwait(false);

         await foreach (var fragment in commandStreamFragments)
         {
            await AppendAIReply(fragment.Message).ConfigureAwait(false);
         }
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

   [GeneratedRegex("(?<=Command:\\s+)\\w+")]
   private static partial Regex ExtractCommandName();
}

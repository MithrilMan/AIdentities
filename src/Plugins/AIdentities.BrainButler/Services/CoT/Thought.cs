namespace AIdentities.BrainButler.Services.CoT;

public class Thought
{
   public string Content { get; set; }
   public string? Response { get; set; }

   public Thought(string content)
   {
      Content = content;
   }

   public async Task SendToLLM(ICompletionConnector completionConnector, CancellationToken cancellationToken)
   {
      var completionResponse = await completionConnector.RequestCompletionAsync(new DefaultCompletionRequest()
      {
         Prompt = Content
      }, cancellationToken).ConfigureAwait(false);

      Response = completionResponse?.GeneratedMessage;
   }
}

namespace AIdentities.Connector.TextGeneration.Models.API;

public record CompletionRequest: CommonRequestParameters
{
   [JsonPropertyName("prompt")]
   public string Prompt { get; set; }
   
   public CompletionRequest(string prompt, TextGenerationParameters parameters) : base(parameters)
   {
      Prompt = prompt;
   }
}

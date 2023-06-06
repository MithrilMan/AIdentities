namespace AIdentities.Connector.TextGeneration.Models.API;
public record ChatCompletionRequest : CommonRequestParameters
{
   [JsonPropertyName("prompt")]
   public string Prompt { get; set; }

   public ChatCompletionRequest(string prompt, TextGenerationParameters parameters) : base(parameters)
   {
      Prompt = prompt;
   }
}

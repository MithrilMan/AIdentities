namespace AIdentities.Connector.TextGeneration.Models.API;

public record ChatCompletionResponse
{
   [JsonPropertyName("Results")]
   public IEnumerable<Result>? Results { get; set; }
   public record Result()
   {
      [JsonPropertyName("text")]
      public string? Text { get; set; }
   }
}

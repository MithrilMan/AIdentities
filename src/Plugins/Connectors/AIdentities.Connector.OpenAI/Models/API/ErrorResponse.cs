namespace AIdentities.Connector.OpenAI.Models.API;
public record ErrorResponse
{
   [JsonPropertyName("error")]
   public ErrorDetails Error { get; set; } = null!;


   public record ErrorDetails
   {
      [JsonPropertyName("message")]
      public string Message { get; set; } = null!;
      [JsonPropertyName("type")]
      public string Type { get; set; } = null!;
      [JsonPropertyName("param")]
      public string? Param { get; set; }
      [JsonPropertyName("code")]
      public string? Code { get; set; }
   }
}

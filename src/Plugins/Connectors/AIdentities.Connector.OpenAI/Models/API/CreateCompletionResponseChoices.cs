namespace AIdentities.Connector.OpenAI.Models.API;

/// <summary>
/// The response from the chat completion API.
/// </summary>
public record CreateCompletionResponseChoices
{
   /// <summary>
   /// Gets or Sets Index
   /// </summary>
   [JsonPropertyName("index")]
   public int? Index { get; set; }

   /// <summary>
   /// Gets or Sets Message
   /// </summary>
   [JsonPropertyName("text")]
   public string? Text { get; set; } = default!;

   /// <summary>
   /// Gets or Sets FinishReason
   /// </summary>
   [JsonPropertyName("finish_reason")]
   public string FinishReason { get; set; } = default!;

   /// <summary>
   /// The delta that may arrive with a stream response.
   /// For simplicity it's routed to the same property as the message.
   /// </summary>
   [JsonPropertyName("delta")]
   public string? Delta
   {
      get => Text;
      set => Text = value;
   }
}

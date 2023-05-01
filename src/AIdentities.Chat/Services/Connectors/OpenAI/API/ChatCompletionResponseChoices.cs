using System.Text.Json.Serialization;

namespace AIdentities.Chat.Services.Connectors.OpenAI.API;

/// <summary>
/// 
/// </summary>
public record ChatCompletionResponseChoices
{
   /// <summary>
   /// Gets or Sets Index
   /// </summary>
   [JsonPropertyName("index")]
   public int? Index { get; set; }

   /// <summary>
   /// Gets or Sets Message
   /// </summary>
   [JsonPropertyName("message")]
   public ChatCompletionResponseMessage Message { get; set; } = default!;

   /// <summary>
   /// Gets or Sets FinishReason
   /// </summary>
   [JsonPropertyName("finish_reason")]
   public string FinishReason { get; set; } = default!;
}

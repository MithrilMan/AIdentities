using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AIdentities.Chat.Services.Connectors.OpenAI.API;

/// <summary>
/// 
/// </summary>
public record CompletionResponseUsage
{
   /// <summary>
   /// Gets or Sets PromptTokens
   /// </summary>
   [Required]
   [JsonPropertyName("prompt_tokens")]
   public int? PromptTokens { get; set; }

   /// <summary>
   /// Gets or Sets CompletionTokens
   /// </summary>
   [Required]
   [JsonPropertyName("completion_tokens")]
   public int? CompletionTokens { get; set; }

   /// <summary>
   /// Gets or Sets TotalTokens
   /// </summary>
   [Required]
   [JsonPropertyName("total_tokens")]
   public int? TotalTokens { get; set; }
}

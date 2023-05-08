using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AIdentities.Chat.Services.Connectors.OpenAI.API;

public record ChatCompletionResponseMessage
{
   /// <summary>
   /// The role of the author of this message.
   /// </summary>
   [Required]
   [JsonPropertyName("role")]
   public ChatCompletionRoleEnum? Role { get; set; }

   /// <summary>
   /// The contents of the message
   /// </summary>
   [Required]
   [JsonPropertyName("content")]
   public string Content { get; set; } = default!;
}

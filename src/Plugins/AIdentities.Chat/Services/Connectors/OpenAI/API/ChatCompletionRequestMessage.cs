using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AIdentities.Chat.Services.Connectors.OpenAI.API;

/// <summary>
/// 
/// </summary>
public record ChatCompletionRequestMessage
{
   /// <summary>
   /// The role of the author of this message.
   /// </summary>
   /// <value>The role of the author of this message.</value>
   [Required]
   [JsonPropertyName("role")]
   public ChatCompletionRoleEnum? Role { get; set; }

   /// <summary>
   /// The contents of the message
   /// </summary>
   /// <value>The contents of the message</value>
   [Required]
   [JsonPropertyName("content")]
   public string Content { get; set; } = default!;

   /// <summary>
   /// The name of the user in a multi-user chat
   /// </summary>
   /// <value>The name of the user in a multi-user chat</value>
   [JsonPropertyName("name")]
   public string? Name { get; set; }
}

namespace AIdentities.Connector.OpenAI.Models.API;

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

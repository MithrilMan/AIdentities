namespace AIdentities.Connector.OpenAI.Models.API;

public record CreateCompletionResponse
{
   /// <summary>
   /// Gets or Sets Id
   /// </summary>
   [Required]
   [JsonPropertyName("id")]
   public string Id { get; set; } = default!;

   /// <summary>
   /// Gets or Sets _Object
   /// </summary>
   [Required]
   [JsonPropertyName("object")]
   public string Object { get; set; } = default!;

   /// <summary>
   /// Gets or Sets Created
   /// </summary>
   [Required]
   [JsonPropertyName("created")]
   public int? Created { get; set; } = default!;

   /// <summary>
   /// Gets or Sets Model
   /// </summary>
   [Required]
   [JsonPropertyName("model")]
   public string Model { get; set; } = default!;

   /// <summary>
   /// Gets or Sets Choices
   /// </summary>
   [Required]
   [JsonPropertyName("choices")]
   public List<CreateCompletionResponseChoices> Choices { get; set; } = new();

   /// <summary>
   /// Gets or Sets Usage
   /// </summary>
   [JsonPropertyName("usage")]
   public CreateCompletionResponseUsage? Usage { get; set; }
}

using System.Text.Json.Serialization;

namespace AIdentities.BooruAIdentityImporter.Models;

public record BooruMetadata
{
   [JsonPropertyName("name")]
   public string? Name { get; set; }

   [JsonPropertyName("description")]
   public string? Description { get; set; }

   [JsonPropertyName("personality")]
   public string? Personality { get; set; }

   [JsonPropertyName("scenario")]
   public string? Scenario { get; set; }

   [JsonPropertyName("first_mes")]
   public string? First_Mes { get; set; }

   [JsonPropertyName("mes_example")]
   public string? Mes_Example { get; set; }

   [JsonPropertyName("metadata")]
   public Dictionary<string, object> Metadata { get; set; } = new();
}

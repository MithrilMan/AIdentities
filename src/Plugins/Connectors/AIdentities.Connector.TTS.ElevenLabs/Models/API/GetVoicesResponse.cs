namespace AIdentities.Connector.TTS.ElevenLabs.Models.API;

/// <summary>
/// We map only what we are interested into, the API response is much more complex than this.
/// </summary>
public sealed record GetVoicesResponse
{
   [JsonPropertyName("voices")]
   public List<Voice> Voices { get; init; } = new();

   public sealed record Voice
   {
      [JsonPropertyName("voice_id")]
      public string? Id { get; init; }

      [JsonPropertyName("name")]
      public string? Name { get; init; }

      [JsonPropertyName("category")]
      public string? Category { get; init; }
   }
}

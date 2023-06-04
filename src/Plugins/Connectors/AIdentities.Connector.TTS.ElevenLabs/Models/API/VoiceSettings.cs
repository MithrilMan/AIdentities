namespace AIdentities.Connector.TTS.ElevenLabs.Models.API;

public sealed record VoiceSettings
{
   [JsonConstructor]
   public VoiceSettings(float stability, float similarityBoost)
   {
      Stability = stability;
      SimilarityBoost = similarityBoost;
   }

   [JsonPropertyName("stability")]
   public float Stability { get; }

   [JsonPropertyName("similarity_boost")]
   public float SimilarityBoost { get; }
}

namespace AIdentities.Connector.TTS.ElevenLabs.Models.API;

public sealed record ElevenLabsTextToSpeechRequest
{
   [JsonPropertyName("text")]
   public string? Text { get; init; }

   [JsonPropertyName("model_id")]
   public string? Model { get; init; }

   [JsonPropertyName("voice_settings")]
   public VoiceSettings? VoiceSettings { get; init; }
}

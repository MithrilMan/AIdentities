namespace AIdentities.Shared.Plugins.Connectors.TextToSpeech;

public record DefaultTextToSpeechRequest(AIdentity? AIdentity, string Text) : ITextToSpeechRequest
{
   public AIdentity? AIdentity { get; set; } = AIdentity;
   public string? VoiceId { get; set; }
   public string? ModelId { get; set; }
   public string Text { get; set; } = Text;
   public Dictionary<string, object> CustomOptions { get; set; } = new();
}

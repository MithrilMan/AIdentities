namespace AIdentities.Shared.Plugins.Connectors.TextToSpeech;

public record DefaultTextToSpeechRequest(string Text) : ITextToSpeechRequest
{
   public string? VoiceId { get; set; }
   public string? ModelId { get; set; }
   public string Text { get; set; } = Text;
   public Dictionary<string, object> CustomOptions { get; set; } = new();
}

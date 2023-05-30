namespace AIdentities.Shared.Plugins.Connectors.TextToSpeech;

/// <summary>
/// An interface with minimal properties to be able to convert text to speech.
/// Specific parameters can be added to the CustomOptions dictionary.
/// </summary>
public interface ITextToSpeechRequest
{
   /// <summary>
   /// The AIdentity to use for the conversion.
   /// </summary>
   AIdentity? AIdentity { get; }

   /// <summary>
   /// The text to be converted to speech.
   /// </summary>
   string Text { get; }

   /// <summary>
   /// The model id to use for the conversion.
   /// It depends on the connector, not all connectors support this.
   /// </summary>
   string? ModelId { get; }

   /// <summary>
   /// The voice id to use for the conversion.
   /// It depends on the connector, not all connectors support this.
   /// </summary>
   string? VoiceId { get; }

   /// <summary>
   /// Custom options that can be passed to the connector when there is something specific to pass
   /// to some specific connector.
   /// E.g. for ElevenLabs, it can be the voice settings like stability and similarity_boost, etc...
   /// </summary>
   Dictionary<string, object> CustomOptions { get; }
}

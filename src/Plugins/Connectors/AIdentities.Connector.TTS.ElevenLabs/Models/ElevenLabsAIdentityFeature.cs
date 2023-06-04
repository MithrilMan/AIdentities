namespace AIdentities.Connector.TTS.ElevenLabs.Models;

/// <summary>
/// An AIdentity feature that allows the AIdentity to generate speech using the ElevenLabs TTS.
/// This feature is used to override the default ElevenLabs TTS settings that can be configured in
/// its own configuration settings tab.
/// </summary>
public record ElevenLabsAIdentityFeature : IAIdentityFeature
{
   public const bool DEFAULT_CUSTOMIZE = false;

   /// <summary>
   /// Specify whether the AIdentity should use the default ElevenLabs TTS settings or not.
   /// When set to true, the AIdentity will use the settings specified in this feature.
   /// </summary>
   public bool Customize { get; set; } = DEFAULT_CUSTOMIZE;

   /// <summary>
   /// Specify the model to use for the text to speech.
   /// If it's null, the default VoiceId will be used.
   /// </summary>
   public string? VoiceId { get; set; } = null;


   /// <summary>
   /// The model to use to generate the speech.
   /// If it's null, the default ModelId will be used.
   /// </summary>
   public string? ModelId { get; set; } = null;


   /// <summary>
   /// The voice 
   /// </summary>
   public float? VoiceStability { get; set; } = null;

   public float? VoiceSimilarityBoost { get; set; } = null;
}

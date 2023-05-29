namespace AIdentities.Connector.TTS.ElevenLabs.Models;

public record ElevenLabsSettings : IPluginSettings
{
   public const bool DEFAULT_ENABLED = true;
   public const string DEFAULT_TEXT_TO_SPEECH_MODEL = "eleven_multilingual_v1";
   public const string DEFAULT_API_ENDPOINT = "https://api.elevenlabs.io/v1/";
   public const string DEFAULT_VOICE_ID = "21m00Tcm4TlvDq8ikWAM";
   public const int DEFAULT_TIMEOUT = 30000;
   public const float DEFAULT_VOICE_STABILITY = 0.4f;
   public const float DEFAULT_VOICE_SIMILARITY_BOOST = 0.75f;
   public const StreamingLatencyOptimization DEFAULT_STREAMING_LATENCY_OPTIMIZATION = StreamingLatencyOptimization.Default;

   /// <summary>
   /// Enable or disable the ElevenLabs API.
   /// </summary>
   public bool Enabled { get; set; } = DEFAULT_ENABLED;

   /// <summary>
   /// ElevenLabs API Chat Endpoint.
   /// </summary>
   public Uri ApiEndpoint { get; set; } = new Uri(DEFAULT_API_ENDPOINT);

   /// <summary>
   /// The default model to use if no model has been specified in the request.
   /// </summary>
   public string DefaultTextToSpeechModel { get; set; } = DEFAULT_TEXT_TO_SPEECH_MODEL;

   /// <summary>
   /// The default voice to use if no voice has been specified in the request.
   /// </summary>
   public string DefaultVoiceId { get; set; } = DEFAULT_VOICE_ID;

   /// <summary>
   /// ElevenLabs API Key.
   /// </summary>
   public string? ApiKey { get; set; } = default!;

   /// <summary>
   /// The default timeout for the API.
   /// </summary>
   public int Timeout { get; set; } = DEFAULT_TIMEOUT;

   public StreamingLatencyOptimization StreamingLatencyOptimization { get; set; } = DEFAULT_STREAMING_LATENCY_OPTIMIZATION;

   /// <summary>
   /// The stability determines how stable the voice is and the randomness of each new generation.
   /// Lowering this slider introduces a broader emotional range for the character (also influenced
   /// by the original voice itself).
   /// Setting the slider too low may result in odd performances that are overly random and cause the 
   /// character to speak too quickly.
   /// On the other hand, setting it too high can lead to a monotonous voice with limited emotion.
   /// </summary>
   public float VoiceStability { get; set; } = DEFAULT_VOICE_STABILITY;

   /// <summary>
   /// The similarity dictates how closely the AI should adhere to the original voice when attempting to replicate it.
   /// If the original audio is of poor quality and the similarity slider is set too high, the AI may reproduce artefacts 
   /// or background noise when trying to mimic the voice if those were present in the original recording.
   /// </summary>
   public float VoiceSimilarityBoost { get; set; } = DEFAULT_VOICE_SIMILARITY_BOOST;


   /// <summary>
   /// Caches the available voices from the API.
   /// This property isn't actively used by the plugin, but is used by the UI to display the available voices.
   /// </summary>
   public List<GetVoicesResponse.Voice>? AvailableVoices { get; set; }
}

namespace AIdentities.Connector.TTS.ElevenLabs.Models;

public class ElevenLabsSettings : IPluginSettings
{
   public const bool DEFAULT_ENABLED = true;
   public const string DEFAULT_TEXT_TO_SPEECH_MODEL = "eleven_multilingual_v1";
   public const string DEFAULT_TEXT_TO_SPEECH_ENDPOINT = "https://api.elevenlabs.io/v1/text-to-speech/";
   public const string DEFAULT_VOICE_ID = "21m00Tcm4TlvDq8ikWAM";
   public const int DEFAULT_TIMEOUT = 30000;

   /// <summary>
   /// Enable or disable the ElevenLabs API.
   /// </summary>
   public bool Enabled { get; set; } = DEFAULT_ENABLED;

   /// <summary>
   /// ElevenLabs API Chat Endpoint.
   /// </summary>
   public Uri TextToSpeechEndpoint { get; set; } = new Uri(DEFAULT_TEXT_TO_SPEECH_ENDPOINT);

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
}

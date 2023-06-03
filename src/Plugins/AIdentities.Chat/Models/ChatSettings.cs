namespace AIdentities.Chat.Models;

public class ChatSettings : IPluginSettings
{

   public string? DefaultConnector { get; set; }

   /// <summary>
   /// True if the chat plugin should use text to speech to read messages.
   /// The voice of each AIdentity can be configured in the AIdentity settings.
   /// Not every Text To Speech connector supports voice selection.
   /// </summary>
   public bool EnableTextToSpeech { get; set; }

   /// <summary>
   /// True if the chat plugin should use speech recognition to listen to the user.
   /// </summary>
   public bool EnableSpeechRecognition { get; set; }

   /// <summary>
   /// BCP 47 language tag for the speech recognition culture.
   /// </summary>
   public string? SpeechRecognitionLanguage { get; set; }

   /// <summary>
   /// If TextToSpeech is enabled, this property will determine when the text to speech will be used.
   /// </summary>
   public TextToSpeechMode TextToSpeechMode { get; set; }

   /// <summary>
   /// The default text to speech connector to use when no connector is specified in the AIdentity settings.
   /// </summary>
   public string? DefaultTextToSpeechConnector { get; set; }

   /// <summary>
   /// A list of skills that are enabled for the chat plugin.
   /// This set of skills will be used by the chat plugin to manage conversations through the Chat Keeper AIdentity.
   /// </summary>
   public List<string> EnabledSkills { get; set; } = new();

   /// <summary>
   /// When skills are disabled, conversation will make less use of internal prompts to LLM and will be faster and cheaper (if paid services are used) but
   /// will have less advanced features.
   /// Set this to true to enable skills.
   /// Having this flag set to true but not enabling a single skill will have the same effect as setting this flag to false.
   /// </summary>
   public bool EnableSkills { get; set; }
}

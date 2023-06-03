using System.Globalization;
using AIdentities.Shared.Plugins.Connectors.TextToSpeech;

namespace AIdentities.Chat.Components;
public partial class Settings
{
   public class State : BaseState
   {
      public Dictionary<string, IConversationalConnector> AvailableConnectors { get; internal set; } = new();
      public Dictionary<string, ITextToSpeechConnector> AvailableTTSConnectors { get; internal set; } = new();
      public List<string> AllKnownSkillNames { get; internal set; } = new();

      public string? DefaultConnector { get; set; } = default!;
      public bool EnableTextToSpeech { get; set; }
      public TextToSpeechMode TextToSpeechMode { get; set; }
      public string? DefaultTextToSpeechConnector { get; set; } = default!;
      public bool EnableSkills { get; set; }
      public ICollection<string> EnabledSkills { get; set; } = new HashSet<string>();

      public bool EnableSpeechRecognition { get; set; }
      public string? SpeechRecognitionLanguage { get; set; }
      public CultureInfo SpeechRecognitionCulture { get; set; } = CultureInfo.CurrentCulture;

      public override void SetFormFields(ChatSettings pluginSettings)
      {
         pluginSettings ??= new();
         DefaultConnector = pluginSettings.DefaultConnector;
         EnableSkills = pluginSettings.EnableSkills;
         EnabledSkills = pluginSettings.EnabledSkills.Intersect(AllKnownSkillNames).ToHashSet();
         EnableTextToSpeech = pluginSettings.EnableTextToSpeech;
         EnableSpeechRecognition = pluginSettings.EnableSpeechRecognition;
         DefaultTextToSpeechConnector = pluginSettings.DefaultTextToSpeechConnector;
         TextToSpeechMode = pluginSettings.TextToSpeechMode;
         SpeechRecognitionLanguage = pluginSettings.SpeechRecognitionLanguage;

         try
         {
            SpeechRecognitionCulture = CultureInfo.GetCultureInfo(SpeechRecognitionLanguage ?? CultureInfo.CurrentCulture.Name);
         }
         catch (Exception)
         {
            SpeechRecognitionCulture = CultureInfo.CurrentCulture;
         }
      }
   }
}

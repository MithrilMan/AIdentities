using AIdentities.Connector.TTS.ElevenLabs.Models;
using AIdentities.Connector.TTS.ElevenLabs.Models.API;

namespace AIdentities.Connector.TTS.ElevenLabs.Components;

public partial class Settings
{
   public class State : BaseState
   {
      public bool? Enabled { get; set; }
      public string? DefaultTextToSpeechModel { get; set; }
      public string? DefaultVoiceId { get; set; }
      public string? TextToSpeechEndpoint { get; set; }
      public string? ApiKey { get; set; } = default!;
      public int? Timeout { get; set; }

      public ElevenLabsSettings.VoiceSettings DefaultVoiceSettings { get; set; } = new();

      public override void SetFormFields(ElevenLabsSettings pluginSettings)
      {
         pluginSettings ??= new();
         ApiKey = pluginSettings.ApiKey;
         DefaultTextToSpeechModel = pluginSettings.DefaultTextToSpeechModel;
         DefaultVoiceId = pluginSettings.DefaultVoiceId;
         Enabled = pluginSettings.Enabled;
         TextToSpeechEndpoint = pluginSettings.TextToSpeechEndpoint?.ToString();
         Timeout = pluginSettings.Timeout;

         DefaultVoiceSettings = pluginSettings.DefaultVoiceSettings with { };
      }
   }
}

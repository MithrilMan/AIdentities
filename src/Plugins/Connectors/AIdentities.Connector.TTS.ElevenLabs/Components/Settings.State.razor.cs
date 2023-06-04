namespace AIdentities.Connector.TTS.ElevenLabs.Components;

public partial class Settings
{
   public class State : BaseState
   {
      public bool Enabled { get; set; }
      public string? DefaultTextToSpeechModel { get; set; }
      public string? DefaultVoiceId { get; set; }
      public string? ApiEndpoint { get; set; }
      public string? ApiKey { get; set; } = default!;
      public int? Timeout { get; set; }
      public StreamingLatencyOptimization StreamingLatencyOptimization { get; set; }

      public float? VoiceStability { get; set; }
      public float? VoiceSimilarityBoost { get; set; }

      public List<GetVoicesResponse.Voice>? AvailableVoices { get; set; }

      public string TestingText { get; set; } = "This is a test! Does it sounds good?";

      public override void SetFormFields(ElevenLabsSettings pluginSettings)
      {
         pluginSettings ??= new();
         ApiKey = pluginSettings.ApiKey;
         DefaultTextToSpeechModel = pluginSettings.DefaultTextToSpeechModel;
         DefaultVoiceId = pluginSettings.DefaultVoiceId;
         Enabled = pluginSettings.Enabled;
         ApiEndpoint = pluginSettings.ApiEndpoint?.ToString();
         Timeout = pluginSettings.Timeout;
         StreamingLatencyOptimization = pluginSettings.StreamingLatencyOptimization;

         VoiceStability = pluginSettings.VoiceStability;
         VoiceSimilarityBoost = pluginSettings.VoiceSimilarityBoost;

         AvailableVoices = pluginSettings.AvailableVoices;

         AddDefaultVoiceToAvailableVoices();
      }

      public void AddDefaultVoiceToAvailableVoices()
      {
         if (AvailableVoices != null && DefaultVoiceId != null && !AvailableVoices.Exists(v => v.Id == DefaultVoiceId))
         {
            AvailableVoices.Add(new GetVoicesResponse.Voice
            {
               Category = "Default",
               Id = DefaultVoiceId,
               Name = DefaultVoiceId
            });
         }
      }
   }
}

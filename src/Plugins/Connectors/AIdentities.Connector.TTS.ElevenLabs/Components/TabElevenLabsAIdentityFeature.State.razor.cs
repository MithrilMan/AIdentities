namespace AIdentities.Connector.TTS.ElevenLabs.Components;

public partial class TabElevenLabsAIdentityFeature
{
   class State
   {
      /// <summary>
      /// When true, allow to customize the AIdentity voice
      /// </summary>
      public bool Customize { get; set; }
      public string? ModelId { get; set; }
      public string? VoiceId { get; set; }

      public float VoiceStability { get; set; }
      public float VoiceSimilarityBoost { get; set; }

      public List<GetVoicesResponse.Voice>? AvailableVoices { get; set; }
      public string TestingText { get; set; } = "This is a test! Does it sounds good?";

      internal void SetFormFields(ElevenLabsAIdentityFeature? feature, ElevenLabsSettings settings)
      {
         Customize = feature?.Customize ?? false;

         ModelId = feature?.ModelId;
         VoiceId = feature?.VoiceId;
         VoiceStability = feature?.VoiceStability ?? settings.VoiceStability;
         VoiceSimilarityBoost = feature?.VoiceSimilarityBoost ?? settings.VoiceSimilarityBoost;

         AvailableVoices = settings.AvailableVoices;
         AddDefaultVoiceToAvailableVoices();
      }

      public void AddDefaultVoiceToAvailableVoices()
      {
         if (AvailableVoices != null
            && !string.IsNullOrWhiteSpace(VoiceId)
            && !AvailableVoices.Exists(v => v.Id == VoiceId))
         {
            AvailableVoices.Add(new GetVoicesResponse.Voice
            {
               Category = "Default",
               Id = VoiceId,
               Name = VoiceId
            });
         }
      }
   }

   private readonly State _state = new State();
}

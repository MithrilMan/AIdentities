namespace AIdentities.Connector.TTS.ElevenLabs.Components;

public partial class TabElevenLabsAIdentityFeature
{
   class State
   {
      /// <summary>
      /// When true, allow to customize the AIdentity voice
      /// </summary>
      public bool? Customize { get; set; }
      public string? ModelId { get; set; }
      public string? VoiceId { get; set; }

      public float? VoiceStability { get; set; }
      public float? VoiceSimilarityBoost { get; set; }

      internal void SetFormFields(ElevenLabsAIdentityFeature? feature)
      {
         Customize = feature?.Customize;

         ModelId = feature?.ModelId;
         VoiceId = feature?.VoiceId;
         VoiceStability = feature?.VoiceStability;
         VoiceSimilarityBoost = feature?.VoiceSimilarityBoost;
      }
   }

   private readonly State _state = new State();
}

using AIdentities.Shared.Features.Core.Components;

namespace AIdentities.Connector.TTS.ElevenLabs.Components;
public partial class Settings : BasePluginSettingsTab<ElevenLabsSettings, Settings.State>
{
   MudForm? _form;

   protected override async ValueTask<bool> AreSettingsValid()
   {
      await _form!.Validate().ConfigureAwait(false);
      return _form.IsValid;
   }

   public override Task<ElevenLabsSettings> PerformSavingAsync()
   {
      return Task.FromResult(new ElevenLabsSettings()
      {
         ApiKey = _state.ApiKey,
         DefaultTextToSpeechModel = _state.DefaultTextToSpeechModel ?? ElevenLabsSettings.DEFAULT_TEXT_TO_SPEECH_MODEL,
         DefaultVoiceId = _state.DefaultVoiceId ?? ElevenLabsSettings.DEFAULT_VOICE_ID,
         Enabled = _state.Enabled ?? ElevenLabsSettings.DEFAULT_ENABLED,
         TextToSpeechEndpoint = new Uri(_state.TextToSpeechEndpoint ?? ElevenLabsSettings.DEFAULT_TEXT_TO_SPEECH_ENDPOINT),
         Timeout = _state.Timeout ?? ElevenLabsSettings.DEFAULT_TIMEOUT,

         VoiceStability = _state.VoiceStability ?? ElevenLabsSettings.DEFAULT_VOICE_STABILITY,
         VoiceSimilarityBoost = _state.VoiceSimilarityBoost ?? ElevenLabsSettings.DEFAULT_VOICE_SIMILARITY_BOOST,
      });
   }
}

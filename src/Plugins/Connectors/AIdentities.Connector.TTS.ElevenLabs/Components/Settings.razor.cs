using AIdentities.Connector.TTS.ElevenLabs.Services;
using AIdentities.Shared.Features.Core.Components;
using AIdentities.Shared.Features.Core.Services;
using AIdentities.Shared.Plugins.Connectors;
using AIdentities.Shared.Plugins.Connectors.TextToSpeech;
using AIdentities.Shared.Services.Javascript;
using Microsoft.JSInterop;

namespace AIdentities.Connector.TTS.ElevenLabs.Components;
public partial class Settings : BasePluginSettingsTab<ElevenLabsSettings, Settings.State>
{
   [Inject] private IConnectorsManager<ITextToSpeechConnector> _connector { get; set; } = default!;
   [Inject] private INotificationService NotificationService { get; set; } = default!;
   [Inject] IPlayAudioStream PlayAudioStream { get; set; } = default!;
   [Inject] IPluginSettingsManager PluginSettingsManager { get; set; } = default!;

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
         Enabled = _state.Enabled,
         ApiEndpoint = new Uri(_state.ApiEndpoint ?? ElevenLabsSettings.DEFAULT_API_ENDPOINT),
         Timeout = _state.Timeout ?? ElevenLabsSettings.DEFAULT_TIMEOUT,
         StreamingLatencyOptimization = _state.StreamingLatencyOptimization,
         VoiceStability = _state.VoiceStability ?? ElevenLabsSettings.DEFAULT_VOICE_STABILITY,
         VoiceSimilarityBoost = _state.VoiceSimilarityBoost ?? ElevenLabsSettings.DEFAULT_VOICE_SIMILARITY_BOOST,

         AvailableVoices = _state.AvailableVoices
      });
   }

   async Task RefreshAvailableVoices()
   {
      try
      {
         var voices = await _connector.GetAll().OfType<ElevenLabsTTSConnector>().First().GetVoices().ConfigureAwait(false);
         _state.AvailableVoices = voices?.Voices ?? new List<GetVoicesResponse.Voice> { new GetVoicesResponse.Voice {
            Category = "Default",
            Id = ElevenLabsSettings.DEFAULT_VOICE_ID,
            Name = "Default"
         }};
      }
      catch (Exception)
      {
         NotificationService.ShowError("Cannot obtain voices from ElevenLabs TTS API. Please check your API key and endpoint.");
         _state.AvailableVoices = new List<GetVoicesResponse.Voice> { new GetVoicesResponse.Voice {
            Category = "Default",
            Id = ElevenLabsSettings.DEFAULT_VOICE_ID,
            Name = "Default"
         }};
      }

      _state.AddDefaultVoiceToAvailableVoices();
      await Task.Delay(100).ConfigureAwait(false);
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);
   }

   async Task TryVoice()
   {
      if (_state.DefaultVoiceId != null)
      {
         var savedSettings = await PerformSavingAsync().ConfigureAwait(false);
         await PluginSettingsManager.SetAsync(savedSettings).ConfigureAwait(false);

         try
         {
            await _connector.GetAll()
               .OfType<ElevenLabsTTSConnector>()
               .First()
               .RequestTextToSpeechAsStreamAsync(new DefaultTextToSpeechRequest(null, _state.TestingText)
               {
                  VoiceId = _state.DefaultVoiceId,
                  ModelId = _state.DefaultTextToSpeechModel,
                  CustomOptions = new()
               {
                  { nameof(ElevenLabsSettings.VoiceStability), _state.VoiceStability! },
                  { nameof(ElevenLabsSettings.VoiceSimilarityBoost), _state.VoiceSimilarityBoost! },
               }
               }, async (stream) =>
               {
                  using var streamRef = new DotNetStreamReference(stream: stream);
                  await PlayAudioStream.PlayAudioFileStream(streamRef).ConfigureAwait(false);
               },
            default
            ).ConfigureAwait(false);
         }
         catch (Exception ex)
         {
            NotificationService.ShowError($"Voice error: {ex.Message}");
         }
      }
   }
}

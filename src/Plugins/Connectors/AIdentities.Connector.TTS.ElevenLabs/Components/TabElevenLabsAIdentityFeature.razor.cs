using AIdentities.Shared.Features.Core.Services;
using AIdentities.Shared.Plugins.Connectors.TextToSpeech;
using AIdentities.Shared.Plugins.Connectors;
using AIdentities.Shared.Services.Javascript;
using AIdentities.Connector.TTS.ElevenLabs.Services;
using Microsoft.JSInterop;
using AIdentities.Shared.Features.AIdentities.Models;

namespace AIdentities.Connector.TTS.ElevenLabs.Components;

public partial class TabElevenLabsAIdentityFeature : IAIdentityFeatureTab<ElevenLabsAIdentityFeature>
{
   [Inject] protected INotificationService NotificationService { get; set; } = default!;
   [Inject] private IConnectorsManager<ITextToSpeechConnector> _connector { get; set; } = default!;
   [Inject] private IPlayAudioStream PlayAudioStream { get; set; } = default!;
   [Inject] private IPluginSettingsManager PluginSettingsManager { get; set; } = default!;
   [Parameter] public bool IsChanged { get; set; } = default!;
   [Parameter] public AIdentity AIdentity { get; set; } = default!;
   [Parameter] public ElevenLabsAIdentityFeature Feature { get; set; } = default!;


   MudForm? _form;

   protected override void OnInitialized()
   {
      base.OnInitialized();
      _state.SetFormFields(Feature, PluginSettingsManager.Get<ElevenLabsSettings>());
   }

   protected override void OnParametersSet()
   {
      base.OnParametersSet();
      if (IsChanged)
         _state.SetFormFields(Feature, PluginSettingsManager.Get<ElevenLabsSettings>());
   }

   public async Task<ElevenLabsAIdentityFeature?> SaveAsync()
   {
      await _form!.Validate().ConfigureAwait(false);
      if (!_form.IsValid)
         return null;

      // care about this expression, parenthesis are important
      return (Feature ?? new ElevenLabsAIdentityFeature()) with
      {
         Customize = _state.Customize,
         ModelId = _state.ModelId,
         VoiceId = _state.VoiceId,
         VoiceStability = _state.VoiceStability,
         VoiceSimilarityBoost = _state.VoiceSimilarityBoost,
      };
   }

   public Task UndoChangesAsync()
   {
      _state.SetFormFields(Feature, PluginSettingsManager.Get<ElevenLabsSettings>());
      return Task.CompletedTask;
   }

   async Task<object?> IAIdentityFeatureTab.SaveAsync()
   {
      var result = await SaveAsync().ConfigureAwait(false);
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);
      return result;
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
      var settings = PluginSettingsManager.Get<ElevenLabsSettings>();

      try
      {
         var request = new DefaultTextToSpeechRequest(AIdentity, _state.TestingText)
         {
            VoiceId = string.IsNullOrWhiteSpace(_state.VoiceId) ? settings.DefaultVoiceId : _state.VoiceId,
            ModelId = string.IsNullOrWhiteSpace(_state.ModelId) ? settings.DefaultTextToSpeechModel : _state.ModelId,
            CustomOptions = new()
               {
                  { nameof(ElevenLabsSettings.VoiceStability), _state.VoiceStability },
                  { nameof(ElevenLabsSettings.VoiceSimilarityBoost), _state.VoiceSimilarityBoost },
               }
         };

         await _connector.GetAll().OfType<ElevenLabsTTSConnector>().First().RequestTextToSpeechAsStreamAsync(
            request: request,
            streamConsumer: async (stream) =>
            {
               using var streamRef = new DotNetStreamReference(stream: stream);
               await PlayAudioStream.PlayAudioFileStream(streamRef).ConfigureAwait(false);
            },
            cancellationToken: default
            ).ConfigureAwait(false);
      }
      catch (Exception ex)
      {
         NotificationService.ShowError($"Voice error: {ex.Message}");
      }
   }
}

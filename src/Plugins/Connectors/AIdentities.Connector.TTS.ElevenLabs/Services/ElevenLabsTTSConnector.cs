using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using AIdentities.Connector.TTS.ElevenLabs.Models;
using AIdentities.Connector.TTS.ElevenLabs.Models.API;
using AIdentities.Shared.Features.Core.Services;
using AIdentities.Shared.Plugins.Connectors.TextToSpeech;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.JSInterop;

namespace AIdentities.Connector.TTS.ElevenLabs.Services;
public class ElevenLabsTTSConnector : ITextToSpeechConnector, IDisposable
{
   const string NAME = nameof(ElevenLabsTTSConnector);
   const string DESCRIPTION = "ElevenLabs Text To Speech Connector.";
   /// <summary>
   /// marker of the starting streamed data.
   /// </summary>
   const string STREAM_DATA_MARKER = "data: ";
   static readonly int _streamDataMarkerLength = STREAM_DATA_MARKER.Length;

   readonly ILogger<ElevenLabsTTSConnector> _logger;
   readonly IPluginSettingsManager _settingsManager;

   public bool Enabled => _settingsManager.Get<ElevenLabsSettings>().Enabled;
   public string Name => NAME;
   public string Description => DESCRIPTION;
   public IFeatureCollection Features => new FeatureCollection();

   protected Uri EndPoint => _settingsManager.Get<ElevenLabsSettings>().TextToSpeechEndpoint;
   protected string DefaultModel => _settingsManager.Get<ElevenLabsSettings>().DefaultTextToSpeechModel;

   private HttpClient _client = default!;
   private readonly JsonSerializerOptions _serializerOptions;
   private ElevenLabsSettings _settings = default!;

   public ElevenLabsTTSConnector(ILogger<ElevenLabsTTSConnector> logger, IPluginSettingsManager settingsManager)
   {
      _logger = logger;
      _settingsManager = settingsManager;

      _serializerOptions = new JsonSerializerOptions()
      {
         DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
      };

      _settingsManager.OnSettingsUpdated += OnSettingsUpdated;
      ApplySettings(_settingsManager.Get<ElevenLabsSettings>());
   }

   /// <summary>
   /// If the settings are updated, we need to update the client.
   /// </summary>
   /// <param name="sender"></param>
   /// <param name="settingType"></param>
   private void OnSettingsUpdated(object? sender, IPluginSettings pluginSettings)
   {
      if (pluginSettings is not ElevenLabsSettings settings) return;

      _logger.LogDebug("Settings updated, applying new settings");
      ApplySettings(settings);
   }

   private void ApplySettings(ElevenLabsSettings settings)
   {
      _settings = settings;

      // we can't modify a HttpClient once it's created, so we need to dispose it and create a new one
      _client?.Dispose();
      _client = new HttpClient
      {
         Timeout = TimeSpan.FromMilliseconds(settings.Timeout)
      };
      _client.DefaultRequestHeaders.Add("xi-api-key", settings.ApiKey);
      _client.DefaultRequestHeaders.Add("accept", "audio/mpeg");
   }

   public TFeatureType? GetFeature<TFeatureType>() => Features.Get<TFeatureType>();
   public void SetFeature<TFeatureType>(TFeatureType? feature) => Features.Set(feature);

   public async Task<ITextToSpeechResponse> RequestTextToSpeechAsync(ITextToSpeechRequest request, CancellationToken cancellationToken)
   {
      //ChatCompletionRequest apiRequest = BuildTTSRequest(request, false);

      //_logger.LogDebug("Performing request ${apiRequest}", apiRequest.Messages);
      //var sw = Stopwatch.StartNew();

      //using HttpResponseMessage response = await _client.PostAsJsonAsync(EndPoint, apiRequest, _serializerOptions, cancellationToken: cancellationToken).ConfigureAwait(false);

      //_logger.LogDebug("Request completed: {Response}", await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));

      //if (response.IsSuccessStatusCode)
      //{
      //   var responseData = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);

      //   sw.Stop();
      //   return new DefaultConversationalResponse
      //   {
      //      GeneratedMessage = responseData?.Choices.FirstOrDefault()?.Message?.Content,
      //      PromptTokens = responseData?.Usage?.PromptTokens,
      //      TotalTokens = responseData?.Usage?.TotalTokens,
      //      CompletionTokens = responseData?.Usage?.CompletionTokens,
      //      ResponseTime = sw.Elapsed
      //   };
      //}
      //else
      //{
      //   _logger.LogError("Request failed: {Error}", response.StatusCode);
      //   throw new Exception($"Request failed with status code {response.StatusCode}");
      //}
      throw new NotImplementedException();
   }

   public async Task RequestTextToSpeechAsStreamAsync(ITextToSpeechRequest request, Func<Stream, Task> streamConsumer, CancellationToken cancellationToken)
   {
      var apiRequest = BuildTTSRequest(request);

      _logger.LogDebug("Performing request ${apiRequest}", apiRequest.Text);

      string? voiceId = request.VoiceId;
      if (string.IsNullOrWhiteSpace(voiceId))
      {
         _logger.LogDebug("No voice id specified, using default voice id: {DefaultVoiceId}", _settings.DefaultVoiceId);
         voiceId = _settings.DefaultVoiceId;
      }
      //'https://api.elevenlabs.io/v1/text-to-speech/21m00Tcm4TlvDq8ikWAM/stream?optimize_streaming_latency=0'
      var endpoint = $"{EndPoint}{voiceId}/stream?optimize_streaming_latency=0";
      using var response = await _client.PostAsJsonAsync(endpoint, apiRequest, _serializerOptions, cancellationToken: cancellationToken).ConfigureAwait(false);

      // ensure all is ok
      response.EnsureSuccessStatusCode();

      // read audio stream
      await streamConsumer(await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false)).ConfigureAwait(false);
   }

   /// <summary>
   /// Builds the request to send to the ElevenLabs TTS API.
   /// </summary>
   /// <param name="request">The base TTS request.</param>
   /// <returns>The request to send to the ElevenLabs TTS API.</returns>
   private ElevenLabsTextToSpeechRequest BuildTTSRequest(ITextToSpeechRequest request)
   {
      string? modelId = request.ModelId;
      if (string.IsNullOrWhiteSpace(modelId))
      {
         _logger.LogDebug("No model id specified, using default model id: {DefaultModelId}", _settings.DefaultTextToSpeechModel);
         modelId = _settings.DefaultTextToSpeechModel;
      }

      float? stability = null;
      if (request.CustomOptions.TryGetValue(nameof(ElevenLabsSettings.VoiceSettings.Stability), out object? stabilityvalue))
      {
         stability = stabilityvalue as float?;
      }

      float? similarityBoost = null;
      if (request.CustomOptions.TryGetValue(nameof(ElevenLabsSettings.VoiceSettings.SimilarityBoost), out object? similarityBoostValue))
      {
         similarityBoost = similarityBoostValue as float?;
      }

      return new ElevenLabsTextToSpeechRequest
      {
         Text = request.Text,
         Model = modelId,
         VoiceSettings = new VoiceSettings(
            stability: stability ?? _settings.DefaultVoiceSettings.Stability,
            similarityBoost: similarityBoost ?? _settings.DefaultVoiceSettings.SimilarityBoost
            )
      };
   }

   public void Dispose()
   {
      _settingsManager.OnSettingsUpdated -= OnSettingsUpdated;
   }
}

using System.Net.Http.Headers;
using System.Net.Http.Json;
using AIdentities.Shared.Features.Core.Services;
using AIdentities.Shared.Plugins.Connectors.TextToSpeech;
using Microsoft.AspNetCore.Http.Features;

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

   public bool Enabled => _settings.Enabled;
   public string Name => NAME;
   public string Description => DESCRIPTION;
   public IFeatureCollection Features => new FeatureCollection();

   protected Uri EndPoint => _endPoint;
   protected string DefaultModel => _settings.DefaultTextToSpeechModel;

   private HttpClient _client = default!;
   private readonly JsonSerializerOptions _serializerOptions;
   private ElevenLabsSettings _settings = default!;
   private Uri _endPoint = default!;

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

      // remove the trailing slash if any
      _endPoint = new Uri(_settings.ApiEndpoint.ToString().TrimEnd('/'));

      // we can't modify a HttpClient once it's created, so we need to dispose it and create a new one
      _client?.Dispose();
      _client = new HttpClient
      {
         Timeout = TimeSpan.FromMilliseconds(settings.Timeout)
      };
      _client.DefaultRequestHeaders.Add("xi-api-key", settings.ApiKey);
   }

   public TFeatureType? GetFeature<TFeatureType>() => Features.Get<TFeatureType>();
   public void SetFeature<TFeatureType>(TFeatureType? feature) => Features.Set(feature);

   public async Task<byte[]> RequestTextToSpeechAsync(ITextToSpeechRequest request, CancellationToken cancellationToken)
   {
      var apiRequest = BuildTTSRequest(request, out string? voiceId);

      _logger.LogDebug("Performing request ${apiRequest}", apiRequest.Text);

      var endpoint = $"{EndPoint}/text-to-speech/{voiceId}?optimize_streaming_latency=0";
      using var response = await _client.PostAsJsonAsync(endpoint, apiRequest, _serializerOptions, cancellationToken: cancellationToken).ConfigureAwait(false);

      // ensure all is ok
      response.EnsureSuccessStatusCode();

      // read audio stream
      return await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
   }

   public async Task RequestTextToSpeechAsStreamAsync(ITextToSpeechRequest request, Func<Stream, Task> streamConsumer, CancellationToken cancellationToken)
   {
      var apiRequest = BuildTTSRequest(request, out string? voiceId);

      _logger.LogDebug("Performing request ${apiRequest}", apiRequest.Text);

      var endpoint = $"{EndPoint}/text-to-speech/{voiceId}/stream?optimize_streaming_latency={(int)_settings.StreamingLatencyOptimization}";
      var postRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
      postRequest.Headers.Accept.Clear();
      postRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("audio/mpeg"));
      postRequest.Content = JsonContent.Create(apiRequest, null, _serializerOptions);
      using var response = await _client.SendAsync(postRequest, cancellationToken).ConfigureAwait(false);
      //using var response = await _client.PostAsJsonAsync(endpoint, apiRequest, _serializerOptions, cancellationToken: cancellationToken).ConfigureAwait(false);

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
   private ElevenLabsTextToSpeechRequest BuildTTSRequest(ITextToSpeechRequest request, out string? voiceId)
   {
      var aidentityVoiceSettings = request.AIdentity?.Features.Get<ElevenLabsAIdentityFeature>();

      voiceId = request.VoiceId ?? aidentityVoiceSettings?.VoiceId;
      if (string.IsNullOrWhiteSpace(voiceId))
      {
         _logger.LogDebug("No voice id specified, using default voice id: {DefaultVoiceId}", _settings.DefaultVoiceId);
         voiceId = _settings.DefaultVoiceId;
      }

      string? modelId = request.ModelId ?? aidentityVoiceSettings?.ModelId;
      if (string.IsNullOrWhiteSpace(modelId))
      {
         _logger.LogDebug("No model id specified, using default model id: {DefaultModelId}", _settings.DefaultTextToSpeechModel);
         modelId = _settings.DefaultTextToSpeechModel;
      }

      float? stability = null;
      if (request.CustomOptions.TryGetValue(nameof(ElevenLabsSettings.VoiceStability), out object? stabilityvalue))
      {
         stability = stabilityvalue as float?;
      }
      stability ??= aidentityVoiceSettings?.VoiceStability;

      float? similarityBoost = null;
      if (request.CustomOptions.TryGetValue(nameof(ElevenLabsSettings.VoiceSimilarityBoost), out object? similarityBoostValue))
      {
         similarityBoost = similarityBoostValue as float?;
      }
      similarityBoost ??= aidentityVoiceSettings?.VoiceSimilarityBoost;

      return new ElevenLabsTextToSpeechRequest
      {
         Text = request.Text,
         Model = modelId,
         VoiceSettings = new VoiceSettings(
            stability: stability ?? _settings.VoiceStability,
            similarityBoost: similarityBoost ?? _settings.VoiceSimilarityBoost
            )
      };
   }

   public async Task<GetVoicesResponse?> GetVoices()
   {
      var endpoint = $"{EndPoint}/voices";
      using var response = _client.GetAsync(endpoint).Result;
      response.EnsureSuccessStatusCode();
      return await response.Content.ReadFromJsonAsync<GetVoicesResponse>().ConfigureAwait(false);
   }

   public static async Task<ReadOnlyMemory<byte>> ReadAsStreamAsyncToReadOnlyMemory(Stream stream, CancellationToken cancellationToken)
   {
      byte[] buffer = new byte[4096];
      using var ms = new MemoryStream();
      int bytesRead;
      while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
      {
         await ms.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
      }
      return new ReadOnlyMemory<byte>(ms.ToArray());
   }

   public void Dispose()
   {
      _settingsManager.OnSettingsUpdated -= OnSettingsUpdated;
   }
}

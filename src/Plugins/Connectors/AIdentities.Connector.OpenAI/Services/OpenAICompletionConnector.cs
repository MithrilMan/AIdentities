using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using AIdentities.Connector.OpenAI.Models;
using AIdentities.Shared.Features.Core.Services;
using AIdentities.Shared.Plugins.Connectors.Completion;
using AIdentities.Shared.Utils;
using Microsoft.AspNetCore.Http.Features;
using Polly;
using Polly.Retry;
using static AIdentities.Connector.OpenAI.Services.SseReader;

namespace AIdentities.Connector.OpenAI.Services;

public class OpenAICompletionConnector : ICompletionConnector, IDisposable
{
   const string NAME = nameof(OpenAICompletionConnector);
   const string DESCRIPTION = "OpenAI Completion Connector that uses Completion API.";
   /// <summary>
   /// marker of the starting streamed data.
   /// </summary>
   const string STREAM_DATA_MARKER = "data: ";
   static readonly int _streamDataMarkerLength = STREAM_DATA_MARKER.Length;

   readonly ILogger<OpenAICompletionConnector> _logger;
   readonly IPluginSettingsManager _settingsManager;

   public bool Enabled => _settingsManager.Get<OpenAISettings>().Enabled;
   public string Name => NAME;
   public string Description => DESCRIPTION;
   public IFeatureCollection Features => new FeatureCollection();

   protected Uri EndPoint => _settingsManager.Get<OpenAISettings>().CompletionEndPoint;
   protected string DefaultModel => _settingsManager.Get<OpenAISettings>().DefaultCompletionModel;

   private HttpClient _client = default!;
   private OpenAISettings _settings = default!;
   readonly AsyncRetryPolicy _retryPolicy;
   private readonly JsonSerializerOptions _serializerOptions;

   public OpenAICompletionConnector(ILogger<OpenAICompletionConnector> logger, IPluginSettingsManager settingsManager)
   {
      _logger = logger;
      _settingsManager = settingsManager;

      _serializerOptions = new JsonSerializerOptions()
      {
         DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
      };

      _retryPolicy = CreateRetryPolicy();

      _settingsManager.OnSettingsUpdated += OnSettingsUpdated;
      ApplySettings(_settingsManager.Get<OpenAISettings>());
   }

   /// <summary>
   /// Creates the retry policy for the cognitive engine.
   /// This is applied everytime a call to a generation API fails with an
   /// exception specified in the retry policy.
   /// The default implementation is a simple exponential backoff that tries 3 times
   /// and catch all exceptions.
   /// </summary>
   /// <returns></returns>
   private AsyncRetryPolicy CreateRetryPolicy()
   {
      // Define the retry policy
      var retryPolicy = Policy
         .Handle<Exception>()
         .WaitAndRetryAsync(
         3,
         retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
         (exception, timeSpan, retryCount, context) =>
         {
            _logger.LogWarning("Retry {RetryCount} due to {ExceptionType} with message {Message}",
                               retryCount,
                               exception.GetType().Name,
                               exception.Message);
         });

      return retryPolicy;
   }

   /// <summary>
   /// If the settings are updated, we need to update the client.
   /// </summary>
   /// <param name="sender"></param>
   /// <param name="settingType"></param>
   private void OnSettingsUpdated(object? sender, IPluginSettings pluginSettings)
   {
      if (pluginSettings is not OpenAISettings settings) return;

      _logger.LogDebug("Settings updated, applying new settings");
      ApplySettings(settings);
   }

   private void ApplySettings(OpenAISettings settings)
   {
      _settings = settings;

      // we can't modify a HttpClient once it's created, so we need to dispose it and create a new one
      _client?.Dispose();
      _client = new HttpClient
      {
         Timeout = TimeSpan.FromMilliseconds(settings.Timeout)
      };
      _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
   }

   public TFeatureType? GetFeature<TFeatureType>() => Features.Get<TFeatureType>();
   public void SetFeature<TFeatureType>(TFeatureType? feature) => Features.Set(feature);

   /// <summary>
   /// Builds a <see cref="DefaultCompletionRequest"/> from a <see cref="ICompletionRequest"/>.
   /// </summary>
   /// <param name="request">The <see cref="DefaultCompletionRequest"/> to build from.</param>
   /// <returns>The built <see cref="DefaultCompletionRequest"/>.</returns>
   private CreateCompletionRequest BuildCreateCompletionRequest(DefaultCompletionRequest request, bool requireStream)
   {
      var apiRequest = new CreateCompletionRequest
      {
         FrequencyPenalty = request.RepetitionPenalityRange,
         MaxTokens = request.MaxGeneratedTokens,
         Prompt = request.Prompt,
         Suffix = request.Suffix,
         Model = request.ModelId ?? DefaultModel,
         PresencePenalty = request.RepetitionPenality,
         N = request.CompletionResults,
         Stop = request.StopSequences,
         Stream = requireStream,
         Temperature = request.Temperature,
         TopP = request.TopPSamplings,
         User = request.UserId,
      };

      _logger.DumpAsJson($"Performing {(requireStream ? "streamed" : "")} completion request", apiRequest);
      return apiRequest;
   }

   public void Dispose()
   {
      _settingsManager.OnSettingsUpdated -= OnSettingsUpdated;
   }

   public async Task<DefaultCompletionResponse?> RequestCompletionAsync(DefaultCompletionRequest request, CancellationToken cancellationToken)
   {
      var apiRequest = BuildCreateCompletionRequest(request, false);

      var sw = Stopwatch.StartNew();
      using var response = await _retryPolicy.ExecuteAsync(async () =>
      {
         return await _client.PostAsJsonAsync(EndPoint, apiRequest, _serializerOptions, cancellationToken: cancellationToken).ConfigureAwait(false);
      }).ConfigureAwait(false);

      _logger.LogDebug("Request completed: {Response}", await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));

      if (response.IsSuccessStatusCode)
      {
         var responseData = await response.Content.ReadFromJsonAsync<CreateCompletionResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);

         sw.Stop();
         return new DefaultCompletionResponse
         {
            ModelId = responseData?.Model,
            GeneratedMessage = responseData?.Choices.FirstOrDefault()?.Text,
            PromptTokens = responseData?.Usage?.PromptTokens,
            TotalTokens = responseData?.Usage?.TotalTokens,
            CompletionTokens = responseData?.Usage?.CompletionTokens,
            ResponseTime = sw.Elapsed,
            FinishReason = responseData?.Choices?.FirstOrDefault()?.FinishReason
         };
      }
      else
      {
         _logger.LogError("Request failed: {Error}", response.StatusCode);
         throw new Exception($"Request failed with status code {response.StatusCode}");
      }
   }

   public async IAsyncEnumerable<DefaultCompletionStreamedResponse> RequestCompletionAsStreamAsync(DefaultCompletionRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      CreateCompletionRequest apiRequest = BuildCreateCompletionRequest(request, true);

      SharpToken.GptEncoding? tokenizer = null;
      try
      {
         tokenizer = SharpToken.GptEncoding.GetEncodingForModel(request.ModelId ?? DefaultModel);
      }
      catch { _logger.LogDebug("Unable to find tokenizer for model {ModelId}, token counter not available", request.ModelId); }

      using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, EndPoint)
      {
         Content = JsonContent.Create(apiRequest, null, _serializerOptions)
      };
      httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

      var sw = Stopwatch.StartNew();
      using var response = await _retryPolicy.ExecuteAsync(async () =>
      {
         return await _client.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
      }).ConfigureAwait(false);

      using var sseReader = new SseReader(await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false));
      int cumulativeCompletionTokens = 0;
      while (true && !cancellationToken.IsCancellationRequested)
      {
         SseLine? sseEvent = await sseReader.TryReadSingleFieldEventAsync().ConfigureAwait(false);
         if (sseEvent == null) break;

         ReadOnlyMemory<char> name = sseEvent.Value.FieldName;
         if (!name.Span.SequenceEqual("data".AsSpan())) throw new InvalidDataException();

         ReadOnlyMemory<char> value = sseEvent.Value.FieldValue;
         if (value.Span.SequenceEqual("[DONE]".AsSpan())) yield break;

         var streamedResponse = JsonSerializer.Deserialize<CreateCompletionResponse>(value.Span, (JsonSerializerOptions?)null);

         if (streamedResponse is not null)
         {
            var message = streamedResponse.Choices.FirstOrDefault()?.Text;
            if (message is not null && tokenizer is not null)
            {
               cumulativeCompletionTokens += tokenizer.Encode(message).Count;
            }

            yield return new DefaultCompletionStreamedResponse
            {
               ModelId = streamedResponse.Model,
               GeneratedMessage = message,
               PromptTokens = streamedResponse.Usage?.PromptTokens,
               CumulativeTotalTokens = streamedResponse.Usage?.TotalTokens,
               CumulativeCompletionTokens = cumulativeCompletionTokens,
               CumulativeResponseTime = sw.Elapsed,
               FinishReason = streamedResponse.Choices?.FirstOrDefault()?.FinishReason
            };
         }
      }
   }
}

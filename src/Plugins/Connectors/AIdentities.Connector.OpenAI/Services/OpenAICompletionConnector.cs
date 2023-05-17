using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using AIdentities.Connector.OpenAI.Models;
using AIdentities.Shared.Features.Core.Services;
using AIdentities.Shared.Plugins.Connectors.Completion;
using Microsoft.AspNetCore.Http.Features;

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
   private readonly JsonSerializerOptions _serializerOptions;

   public OpenAICompletionConnector(ILogger<OpenAICompletionConnector> logger, IPluginSettingsManager settingsManager)
   {
      _logger = logger;
      _settingsManager = settingsManager;

      _serializerOptions = new JsonSerializerOptions()
      {
         DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
      };

      _settingsManager.OnSettingsUpdated += OnSettingsUpdated;
      ApplySettings(_settingsManager.Get<OpenAISettings>());
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
   private CreateCompletionRequest BuildCreateCompletionRequest(ICompletionRequest request, bool requireStream) => new CreateCompletionRequest
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

   public void Dispose()
   {
      _settingsManager.OnSettingsUpdated -= OnSettingsUpdated;
   }

   public async Task<ICompletionResponse?> RequestCompletionAsync(ICompletionRequest request, CancellationToken cancellationToken)
   {
      var apiRequest = BuildCreateCompletionRequest(request, false);

      _logger.LogDebug("Performing request ${apiRequest}", apiRequest.Prompt);
      var sw = Stopwatch.StartNew();

      using HttpResponseMessage response = await _client.PostAsJsonAsync(EndPoint, apiRequest, _serializerOptions, cancellationToken: cancellationToken).ConfigureAwait(false);

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

   public async IAsyncEnumerable<ICompletionStreamedResponse> RequestCompletionAsStreamAsync(ICompletionRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      CreateCompletionRequest apiRequest = BuildCreateCompletionRequest(request, true);
      _logger.LogDebug("Performing request ${apiRequest}", apiRequest.Prompt);

      var sw = Stopwatch.StartNew();

      using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, EndPoint)
      {
         Content = JsonContent.Create(apiRequest, null, _serializerOptions)
      };
      httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

      using var httpResponseMessage = await _client.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

      var stream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
      await using var streamScope = stream.ConfigureAwait(false);
      using var streamReader = new StreamReader(stream);
      while (streamReader.EndOfStream is false)
      {
         cancellationToken.ThrowIfCancellationRequested();
         var line = (await streamReader.ReadLineAsync(cancellationToken).ConfigureAwait(false))!;

         if (line.StartsWith(STREAM_DATA_MARKER))
            line = line[_streamDataMarkerLength..];

         if (string.IsNullOrWhiteSpace(line)) continue; //empty line

         if (line == "[DONE]") break;

         CreateCompletionResponse? streamedResponse = null;
         try
         {
            streamedResponse = JsonSerializer.Deserialize<CreateCompletionResponse>(line);
         }
         catch (Exception)
         {
            // if we can't deserialize the response, it's probably because it's an error, try to deserialize
            // the rest of the stream as an error message
            line += await streamReader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            var error = JsonSerializer.Deserialize<ErrorResponse>(line);
            if (error?.Error is not null)
               throw new Exception($"Request failed with status code {error.Error.Code}: {error.Error.Message}");
         }

         if (streamedResponse is not null)
         {
            yield return new DefaultCompletionStreamedResponse
            {
               ModelId = streamedResponse.Model,
               GeneratedMessage = streamedResponse.Choices?.FirstOrDefault()?.Text,
               PromptTokens = streamedResponse.Usage?.PromptTokens,
               CumulativeTotalTokens = streamedResponse.Usage?.TotalTokens,
               CumulativeCompletionTokens = streamedResponse.Usage?.CompletionTokens,
               CumulativeResponseTime = sw.Elapsed,
               FinishReason = streamedResponse.Choices?.FirstOrDefault()?.FinishReason
            };
         }
      }
   }
}

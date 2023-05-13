using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using AIdentities.Shared.Features.Core.Services;
using Microsoft.AspNetCore.Http.Features;

namespace AIdentities.Connector.OpenAI.Services;
public class OpenAIConnector : IConversationalConnector, IDisposable
{
   const string NAME = nameof(OpenAIConnector);
   const string DESCRIPTION = "OpenAI Chat Connector that uses ChatCompletion API.";
   /// <summary>
   /// marker of the starting streamed data.
   /// </summary>
   const string STREAM_DATA_MARKER = "data: ";
   static readonly int _streamDataMarkerLength = STREAM_DATA_MARKER.Length;

   readonly ILogger<OpenAIConnector> _logger;
   readonly IPluginSettingsManager _settingsManager;

   public bool Enabled => _settingsManager.Get<OpenAISettings>().Enabled;
   public Uri EndPoint => _settingsManager.Get<OpenAISettings>().EndPoint;
   public int Timeout => _settingsManager.Get<OpenAISettings>().Timeout;
   public string? ApiKey => _settingsManager.Get<OpenAISettings>().ApiKey;
   public string DefaultModel => _settingsManager.Get<OpenAISettings>().DefaultModel;

   public string Name => NAME;
   public string Description => DESCRIPTION;
   public IFeatureCollection Features => new FeatureCollection();

   private HttpClient _client;
   private readonly JsonSerializerOptions _serializerOptions;

   public OpenAIConnector(ILogger<OpenAIConnector> logger, IPluginSettingsManager settingsManager)
   {
      _logger = logger;
      _settingsManager = settingsManager;

      _client = CreateHttpClient();
      _serializerOptions = new JsonSerializerOptions()
      {
         DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
      };

      _settingsManager.OnSettingsUpdated += OnSettingsUpdated;
   }

   // if the settings are updated, we need to update the client
   private void OnSettingsUpdated(object? sender, Type settingType)
   {
      if (settingType == typeof(OpenAISettings))
      {
         // we can't modify a HttpClient once it's created, so we need to dispose it and create a new one
         _client?.Dispose();
         _client = CreateHttpClient();
      }
   }

   public TFeatureType? GetFeature<TFeatureType>() => Features.Get<TFeatureType>();
   public void SetFeature<TFeatureType>(TFeatureType? feature) => Features.Set(feature);

   public async Task<IConversationalResponse?> RequestChatCompletionAsync(IConversationalRequest request)
   {
      ChatCompletionRequest apiRequest = BuildChatCompletionRequest(request, false);

      _logger.LogDebug("Performing request ${apiRequest}", apiRequest.Messages);
      var sw = Stopwatch.StartNew();

      using HttpResponseMessage response = await _client.PostAsJsonAsync(EndPoint, apiRequest, _serializerOptions).ConfigureAwait(false);

      _logger.LogDebug("Request completed: {Request}", await response.RequestMessage!.Content!.ReadAsStringAsync().ConfigureAwait(false));

      if (response.IsSuccessStatusCode)
      {
         _logger.LogDebug("Request succeeded: {ResponseContent}", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
         var responseData = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>().ConfigureAwait(false);

         sw.Stop();
         return new ConversationalResponse
         {
            GeneratedMessage = responseData?.Choices.FirstOrDefault()?.Message?.Content,
            PromptTokens = responseData?.Usage?.PromptTokens,
            TotalTokens = responseData?.Usage?.TotalTokens,
            CompletionTokens = responseData?.Usage?.CompletionTokens,
            ResponseTime = sw.Elapsed
         };
      }
      else
      {
         _logger.LogError("Request failed: {Error}", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
         throw new Exception($"Request failed with status code {response.StatusCode}");
      }
   }

   public async IAsyncEnumerable<IConversationalStreamedResponse> RequestChatCompletionAsStreamAsync(IConversationalRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      ChatCompletionRequest apiRequest = BuildChatCompletionRequest(request, true);
      _logger.LogDebug("Performing request ${apiRequest}", apiRequest.Messages);

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

         ChatCompletionResponse? streamedResponse = null;
         try
         {
            streamedResponse = JsonSerializer.Deserialize<ChatCompletionResponse>(line);
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
            yield return new ConversationalStreamedResponse
            {
               GeneratedMessage = streamedResponse?.Choices.FirstOrDefault()?.Message?.Content,
               PromptTokens = streamedResponse?.Usage?.PromptTokens,
               CumulativeTotalTokens = streamedResponse?.Usage?.TotalTokens,
               CumulativeCompletionTokens = streamedResponse?.Usage?.CompletionTokens,
               CumulativeResponseTime = sw.Elapsed
            };
         }
      }
   }

   private HttpClient CreateHttpClient()
   {
      var client = new HttpClient
      {
         Timeout = TimeSpan.FromMilliseconds(Timeout)
      };

      client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

      return client;
   }

   /// <summary>
   /// Builds a <see cref="ChatCompletionRequest"/> from a <see cref="ChatApiRequest"/>.
   /// </summary>
   /// <param name="request">The <see cref="ChatApiRequest"/> to build from.</param>
   /// <returns>The built <see cref="ChatCompletionRequest"/>.</returns>
   private ChatCompletionRequest BuildChatCompletionRequest(IConversationalRequest request, bool requireStream) => new ChatCompletionRequest
   {
      FrequencyPenalty = request.RepetitionPenalityRange,
      MaxTokens = request.MaxGeneratedTokens,
      Messages = request.Messages.Select(m => new ChatCompletionRequestMessage
      {
         Content = m.Content,
         Name = m.Name,
         Role = MapRole(m.Role)
      }).ToList(),
      Model = request.ModelId ?? DefaultModel,
      PresencePenalty = request.RepetitionPenality,
      N = request.CompletionResults,
      Stop = request.StopSequences,
      Stream = requireStream,
      Temperature = request.Temperature,
      TopP = request.TopPSamplings,
      User = request.UserId,
   };

   private static ChatCompletionRoleEnum? MapRole(ConversationalRole role) => role switch
   {
      ConversationalRole.User => ChatCompletionRoleEnum.User,
      ConversationalRole.Assistant => ChatCompletionRoleEnum.Assistant,
      ConversationalRole.System => ChatCompletionRoleEnum.System,
      _ => throw new NotImplementedException()
   };

   public void Dispose()
   {
      _settingsManager.OnSettingsUpdated -= OnSettingsUpdated;
   }
}

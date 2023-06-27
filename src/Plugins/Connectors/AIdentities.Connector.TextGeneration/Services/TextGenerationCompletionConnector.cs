using System.Diagnostics;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using AIdentities.Shared.Features.Core.Services;
using AIdentities.Shared.Plugins.Connectors.Completion;
using Microsoft.AspNetCore.Http.Features;
using Polly;
using Polly.Retry;

namespace AIdentities.Connector.TextGeneration.Services;
public class TextGenerationCompletionConnector : ICompletionConnector, IDisposable
{
   const string NAME = nameof(TextGenerationChatConnector);
   const string DESCRIPTION = "TextGeneration Chat Connector that uses ChatCompletion API.";
   const string ASSISTANT_ROLE_PREFIX = "\nAssistant: ";

   /// <summary>
   /// marker of the starting streamed data.
   /// </summary>
   const string STREAM_DATA_MARKER = "data: ";
   static readonly int _streamDataMarkerLength = STREAM_DATA_MARKER.Length;

   readonly ILogger<TextGenerationChatConnector> _logger;
   readonly IPluginSettingsManager _settingsManager;

   public bool Enabled => _settingsManager.Get<TextGenerationSettings>().Enabled;
   protected Uri EndPoint => _settingsManager.Get<TextGenerationSettings>().CompletionEndPoint;
   protected Uri StreamedEndPoint => _settingsManager.Get<TextGenerationSettings>().StreamedCompletionEndPoint;
   public string DefaultModel => _settingsManager.Get<TextGenerationSettings>().DefaultModel;
   public TextGenerationParameters DefaultParameters => _settingsManager.Get<TextGenerationSettings>().DefaultParameters;


   public string Name => NAME;
   public string Description => DESCRIPTION;
   public IFeatureCollection Features => new FeatureCollection();

   private HttpClient _client = default!;
   readonly AsyncRetryPolicy _retryPolicy;
   private readonly JsonSerializerOptions _serializerOptions;

   public TextGenerationCompletionConnector(ILogger<TextGenerationChatConnector> logger, IPluginSettingsManager settingsManager)
   {
      _logger = logger;
      _settingsManager = settingsManager;

      _serializerOptions = new JsonSerializerOptions()
      {
         DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
      };

      _retryPolicy = CreateRetryPolicy();

      _settingsManager.OnSettingsUpdated += OnSettingsUpdated;
      ApplySettings(_settingsManager.Get<TextGenerationSettings>());
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

   private void ApplySettings(TextGenerationSettings settings)
   {
      // we can't modify a HttpClient once it's created, so we need to dispose it and create a new one
      _client?.Dispose();
      _client = new HttpClient
      {
         Timeout = TimeSpan.FromMilliseconds(settings.Timeout)
      };
   }

   /// <summary>
   /// If the settings are updated, we need to update the client.
   /// </summary>
   /// <param name="sender"></param>
   /// <param name="settingType"></param>
   private void OnSettingsUpdated(object? sender, IPluginSettings pluginSettings)
   {
      if (pluginSettings is not TextGenerationSettings settings) return;

      _logger.LogDebug("Settings updated, applying new settings");
      ApplySettings(settings);
   }

   public TFeatureType? GetFeature<TFeatureType>() => Features.Get<TFeatureType>();
   public void SetFeature<TFeatureType>(TFeatureType? feature) => Features.Set(feature);

   public async Task<Shared.Plugins.Connectors.Completion.CompletionResponse?> RequestCompletionAsync(Shared.Plugins.Connectors.Completion.CompletionRequest request, CancellationToken cancellationToken)
   {
      var apiRequest = BuildCompletionRequest(request);

      var sw = Stopwatch.StartNew();
      try
      {
         JsonContent content = JsonContent.Create(apiRequest, mediaType: null, null);

         // oobabooga TextGeneration API implementation requires content-lenght
         await content.LoadIntoBufferAsync().ConfigureAwait(false);
         using var response = await _retryPolicy.ExecuteAsync(async () =>
         {
            return await _client.PostAsync(EndPoint, content, cancellationToken).ConfigureAwait(false);
         }).ConfigureAwait(false);

         _logger.DumpAsJson("Request completed", await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));

         if (response.IsSuccessStatusCode)
         {
            var responseData = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);

            sw.Stop();
            return new Shared.Plugins.Connectors.Completion.CompletionResponse
            {
               GeneratedMessage = CleanUpResponse(responseData),
               PromptTokens = default,
               TotalTokens = default,
               CompletionTokens = default,
               ResponseTime = sw.Elapsed
            };
         }
         else
         {
            _logger.LogError("Request failed: {Error}", response.StatusCode);
            throw new Exception($"Request failed with status code {response.StatusCode}");
         }
      }
      catch (Exception ex)
      {
         _logger.LogError(ex, "Request failed");
         throw;
      }
   }

   private static string? CleanUpResponse(ChatCompletionResponse? responseData)
   {
      var response = responseData?.Results?.FirstOrDefault()?.Text;
      if (response is null) return null;

      // remove the eventual assistant reference from the response
      // TODO: with a multi-character chat, we should remove the user names.
      if (response.StartsWith(ASSISTANT_ROLE_PREFIX))
      {
         return response[ASSISTANT_ROLE_PREFIX.Length..];
      }

      return response;
   }

   public IAsyncEnumerable<CompletionStreamedResponse> RequestCompletionAsStreamAsync(Shared.Plugins.Connectors.Completion.CompletionRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      var sw = Stopwatch.StartNew();

      return ProcessStreamResponse(
         request: BuildCompletionRequest(request),
         stopWatch: sw,
         cancellationToken: cancellationToken
         );
   }

   private async IAsyncEnumerable<CompletionStreamedResponse> ProcessStreamResponse(
      Models.API.CompletionRequest request,
      Stopwatch stopWatch,
      [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      using ClientWebSocket webSocket = await _retryPolicy.ExecuteAsync(async () =>
      {
         var client = new ClientWebSocket();
         try
         {
            await client.ConnectAsync(StreamedEndPoint, cancellationToken).ConfigureAwait(false);
            await client.SendAsync(JsonSerializer.SerializeToUtf8Bytes(request), WebSocketMessageType.Text, true, cancellationToken).ConfigureAwait(false);
         }
         catch (Exception)
         {
            client?.Dispose();
            throw;
         }

         return client;
      }).ConfigureAwait(false);

      byte[] receiveBuffer = new byte[1024];
      while (webSocket.State == WebSocketState.Open)
      {
         var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), cancellationToken).ConfigureAwait(false);
         if (result.MessageType == WebSocketMessageType.Text)
         {
            var response = JsonDocument.Parse(receiveBuffer.AsMemory(0, result.Count));

            //nothing to do, we expect an event. either "text_stream" or "stream_end"
            if (!response.RootElement.TryGetProperty("event", out var eventType)) yield break;

            switch (eventType.GetString())
            {
               case "stream_end":
                  await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Stream completed", cancellationToken).ConfigureAwait(false);
                  yield break;
               case "text_stream":
                  var generatedText = response.RootElement.GetProperty("text").GetString();
                  yield return new CompletionStreamedResponse
                  {
                     GeneratedMessage = generatedText,
                     PromptTokens = null,
                     CumulativeTotalTokens = null, //TODO use a proper tokenizer
                     CumulativeCompletionTokens = null,
                     CumulativeResponseTime = stopWatch.Elapsed
                  };
                  break;
               default:
                  _logger.LogError("Unexpected event type: {EventType}", eventType.GetString());
                  yield break;
            }
         }
      }
   }

   /// <summary>
   /// Builds a <see cref="ChatCompletionRequest"/> from a <see cref="ChatApiRequest"/>.
   /// </summary>
   /// <param name="request">The <see cref="ChatApiRequest"/> to build from.</param>
   /// <returns>The built <see cref="ChatCompletionRequest"/>.</returns>
   private Models.API.CompletionRequest BuildCompletionRequest(Shared.Plugins.Connectors.Completion.CompletionRequest request)
   {
      var defaultParameters = DefaultParameters;

      var completionRequest = new Models.API.CompletionRequest(request.Prompt, defaultParameters);

      if (request.MaxGeneratedTokens != null) { completionRequest.MaxNewTokens = request.MaxGeneratedTokens; }
      if (request.RepetitionPenality != null) { completionRequest.RepetitionPenalty = request.RepetitionPenality; }
      if (request.RepetitionPenalityRange != null) { completionRequest.EncoderRepetitionPenalty = request.RepetitionPenalityRange; }
      if (request.StopSequences != null) { completionRequest.StoppingStrings = request.StopSequences; }
      if (request.Temperature != null) { completionRequest.Temperature = request.Temperature; }
      if (request.TopKSamplings != null) { completionRequest.TopK = (int?)request.TopKSamplings; }
      if (request.TopPSamplings != null) { completionRequest.TopP = request.TopPSamplings; }
      if (request.TypicalSampling != null) { completionRequest.TypicalP = request.TypicalSampling; }
      if (request.TopASamplings != null) { completionRequest.TopA = request.TopASamplings; }
      // ignored properties by TextGeneration API
      // request.CompletionResults
      // request.ContextSize
      // request.LogitBias
      // request.ModelId
      // request.TailFreeSampling

      _logger.DumpAsJson("Performing completion request", completionRequest);

      return completionRequest;
   }

   public void Dispose()
   {
      _settingsManager.OnSettingsUpdated -= OnSettingsUpdated;
   }
}

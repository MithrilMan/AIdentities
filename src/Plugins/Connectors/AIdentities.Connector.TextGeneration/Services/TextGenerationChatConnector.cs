using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using AIdentities.Shared.Features.AIdentities.Models;
using AIdentities.Shared.Features.Core.Services;
using Microsoft.AspNetCore.Http.Features;
using Polly;
using Polly.Retry;

namespace AIdentities.Connector.TextGeneration.Services;
public class TextGenerationChatConnector : IConversationalConnector, IDisposable
{
   const string NAME = nameof(TextGenerationChatConnector);
   const string DESCRIPTION = "TextGeneration Chat Connector that uses ChatCompletion API.";

   /// <summary>
   /// marker of the starting streamed data.
   /// </summary>
   const string STREAM_DATA_MARKER = "data: ";
   static readonly int _streamDataMarkerLength = STREAM_DATA_MARKER.Length;

   readonly ILogger<TextGenerationChatConnector> _logger;
   readonly IPluginSettingsManager _settingsManager;

   public bool Enabled => _settingsManager.Get<TextGenerationSettings>().Enabled;
   protected Uri ChatEndPoint => _settingsManager.Get<TextGenerationSettings>().CompletionEndPoint;
   protected Uri ChatStreamedEndPoint => _settingsManager.Get<TextGenerationSettings>().StreamedCompletionEndPoint;
   public string DefaultModel => _settingsManager.Get<TextGenerationSettings>().DefaultModel;
   public TextGenerationParameters DefaultParameters => _settingsManager.Get<TextGenerationSettings>().DefaultParameters;


   public string Name => NAME;
   public string Description => DESCRIPTION;
   public IFeatureCollection Features => new FeatureCollection();

   private HttpClient _client = default!;
   readonly AsyncRetryPolicy _retryPolicy;
   private readonly JsonSerializerOptions _serializerOptions;

   public TextGenerationChatConnector(ILogger<TextGenerationChatConnector> logger, IPluginSettingsManager settingsManager)
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

   public async Task<IConversationalResponse?> RequestChatCompletionAsync(IConversationalRequest request, CancellationToken cancellationToken)
   {
      ChatCompletionRequest apiRequest = BuildChatCompletionRequest(request);

      var sw = Stopwatch.StartNew();

      try
      {
         JsonContent content = JsonContent.Create(apiRequest, mediaType: null, null);
         // oobabooga TextGeneration API implementation requires content-lenght
         await content.LoadIntoBufferAsync().ConfigureAwait(false);
         using var response = await _retryPolicy.ExecuteAsync(async () =>
         {
            return await _client.PostAsync(ChatEndPoint, content, cancellationToken).ConfigureAwait(false);
         }).ConfigureAwait(false);

         _logger.DumpAsJson("Request completed", await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));

         if (response.IsSuccessStatusCode)
         {
            var responseData = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);

            sw.Stop();
            return new DefaultConversationalResponse
            {
               GeneratedMessage = CleanUpResponse(responseData, request.AIdentity),
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

   static string GetResponsePrefix(AIdentity aIdentity) => $"{aIdentity.Name}:";

   private static string? CleanUpResponse(ChatCompletionResponse? responseData, AIdentity aIdentity)
   {
      var response = responseData?.Results?.FirstOrDefault()?.Text;
      if (response is null) return null;

      string textToSkip = GetResponsePrefix(aIdentity);
      if (response.StartsWith(textToSkip))
      {
         return response[textToSkip.Length..];
      }

      return response;
   }

   public IAsyncEnumerable<IConversationalStreamedResponse> RequestChatCompletionAsStreamAsync(IConversationalRequest request, CancellationToken cancellationToken)
   {
      var sw = Stopwatch.StartNew();

      return ProcessStreamResponse(
         request: BuildChatCompletionRequest(request),
         stopWatch: sw,
         cancellationToken: cancellationToken
         );
   }

   private async IAsyncEnumerable<IConversationalStreamedResponse> ProcessStreamResponse(
      ChatCompletionRequest request,
      Stopwatch stopWatch,
      [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      using ClientWebSocket webSocket = await _retryPolicy.ExecuteAsync(async () =>
      {
         var client = new ClientWebSocket();
         try
         {
            await client.ConnectAsync(ChatStreamedEndPoint, cancellationToken).ConfigureAwait(false);
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
                  yield break;
               case "text_stream":
                  var generatedText = response.RootElement.GetProperty("text").GetString();
                  yield return new DefaultConversationalStreamedResponse
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
   private ChatCompletionRequest BuildChatCompletionRequest(IConversationalRequest request)
   {
      var defaultParameters = DefaultParameters;

      var instructions = request.Messages.Where(request => request.Role == DefaultConversationalRole.System).Select(request => request.Content).ToList();

      // we need now to build the prompt since textgeneration api has just a single prompt field, not a list of messages with roles
      var sb = new StringBuilder();
      foreach (var instruction in instructions.Take(1))
      {
         sb.AppendLine(instruction);
      }


      foreach (var message in request.Messages)
      {
         // we add a new line to space more the messages belonging to different roles, not sure if it's needed
         if (message.Role == DefaultConversationalRole.System)
         {
            continue;
            //sb.AppendLine($"{message.Content}");
         }
         else
         {
            sb.AppendLine($"{message.Name}: {message.Content}");
         }
      }
      sb.Append(GetResponsePrefix(request.AIdentity)); //append already the assistant role, so the completion will start from here and we can remove it later

      var chatRequest = new ChatCompletionRequest(
         prompt: sb.ToString(),
         parameters: defaultParameters
         );

      if (request.MaxGeneratedTokens != null) { chatRequest.MaxNewTokens = request.MaxGeneratedTokens; }
      if (request.RepetitionPenality != null) { chatRequest.RepetitionPenalty = request.RepetitionPenality; }
      if (request.RepetitionPenalityRange != null) { chatRequest.EncoderRepetitionPenalty = request.RepetitionPenalityRange; }
      if (request.StopSequences != null) { chatRequest.StoppingStrings = request.StopSequences; }
      if (request.Temperature != null) { chatRequest.Temperature = request.Temperature; }
      if (request.TopKSamplings != null) { chatRequest.TopK = (int?)request.TopKSamplings; }
      if (request.TopPSamplings != null) { chatRequest.TopP = request.TopPSamplings; }
      if (request.TypicalSampling != null) { chatRequest.TypicalP = request.TypicalSampling; }
      if (request.TopASamplings != null) { chatRequest.TopA = request.TopASamplings; }
      // ignored properties by TextGeneration API
      // request.CompletionResults
      // request.ContextSize
      // request.LogitBias
      // request.ModelId
      // request.TailFreeSampling

      _logger.DumpAsJson("Performing chat request", chatRequest);

      return chatRequest;
   }

   public void Dispose()
   {
      _settingsManager.OnSettingsUpdated -= OnSettingsUpdated;
   }
}

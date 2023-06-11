using System;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using AIdentities.Shared.Features.Core.Services;
using AIdentities.Shared.Plugins.Connectors.Completion;
using Microsoft.AspNetCore.Http.Features;

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
   private readonly JsonSerializerOptions _serializerOptions;

   public TextGenerationCompletionConnector(ILogger<TextGenerationChatConnector> logger, IPluginSettingsManager settingsManager)
   {
      _logger = logger;
      _settingsManager = settingsManager;

      _serializerOptions = new JsonSerializerOptions()
      {
         DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
      };

      _settingsManager.OnSettingsUpdated += OnSettingsUpdated;
      ApplySettings(_settingsManager.Get<TextGenerationSettings>());
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

   public async Task<ICompletionResponse?> RequestCompletionAsync(ICompletionRequest request, CancellationToken cancellationToken)
   {
      var apiRequest = BuildCompletionRequest(request, false);

      _logger.DumpAsJson("Performing request", apiRequest);

      var sw = Stopwatch.StartNew();
      try
      {
         JsonContent content = JsonContent.Create(apiRequest, mediaType: null, null);

         // oobabooga TextGeneration API implementation requires content-lenght
         await content.LoadIntoBufferAsync().ConfigureAwait(false);
         using HttpResponseMessage response = await _client.PostAsync(EndPoint, content, cancellationToken).ConfigureAwait(false);

         _logger.DumpAsJson("Request completed", await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));

         if (response.IsSuccessStatusCode)
         {
            var responseData = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);

            sw.Stop();
            return new DefaultCompletionResponse
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

   public async IAsyncEnumerable<ICompletionStreamedResponse> RequestCompletionAsStreamAsync(ICompletionRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      var apiRequest = BuildCompletionRequest(request, false);

      _logger.DumpAsJson("Performing stream request", apiRequest.Prompt);
      var sw = Stopwatch.StartNew();

      using ClientWebSocket webSocket = new ClientWebSocket();
      await webSocket.ConnectAsync(StreamedEndPoint, cancellationToken).ConfigureAwait(false);

      await webSocket.SendAsync(JsonSerializer.SerializeToUtf8Bytes(apiRequest), WebSocketMessageType.Text, true, cancellationToken).ConfigureAwait(false);

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
                  yield return new DefaultCompletionStreamedResponse
                  {
                     GeneratedMessage = generatedText,
                     PromptTokens = null,
                     CumulativeTotalTokens = null, //TODO use a proper tokenizer
                     CumulativeCompletionTokens = null,
                     CumulativeResponseTime = sw.Elapsed
                  };
                  break;
               default:
                  _logger.LogError("Unexpected event type: {EventType}", eventType.GetString());
                  yield break;
            }
         }
      }

      sw.Stop();
   }

   /// <summary>
   /// Builds a <see cref="ChatCompletionRequest"/> from a <see cref="ChatApiRequest"/>.
   /// </summary>
   /// <param name="request">The <see cref="ChatApiRequest"/> to build from.</param>
   /// <returns>The built <see cref="ChatCompletionRequest"/>.</returns>
   private CompletionRequest BuildCompletionRequest(ICompletionRequest request, bool requireStream)
   {
      var defaultParameters = DefaultParameters;

      var completionRequest = new CompletionRequest(request.Prompt, defaultParameters);

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

      return completionRequest;
   }

   public void Dispose()
   {
      _settingsManager.OnSettingsUpdated -= OnSettingsUpdated;
   }
}

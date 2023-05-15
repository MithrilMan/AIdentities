using System.Diagnostics;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using AIdentities.Shared.Features.Core.Services;
using Microsoft.AspNetCore.Http.Features;

namespace AIdentities.Connector.TextGeneration.Services;
public class TextGenerationConnector : IConversationalConnector, IDisposable
{
   const string NAME = nameof(TextGenerationConnector);
   const string DESCRIPTION = "TextGeneration Chat Connector that uses ChatCompletion API.";
   const string ASSISTANT_ROLE_PREFIX = "\nAssistant: ";

   /// <summary>
   /// marker of the starting streamed data.
   /// </summary>
   const string STREAM_DATA_MARKER = "data: ";
   static readonly int _streamDataMarkerLength = STREAM_DATA_MARKER.Length;

   readonly ILogger<TextGenerationConnector> _logger;
   readonly IPluginSettingsManager _settingsManager;

   public bool Enabled => _settingsManager.Get<TextGenerationSettings>().Enabled;
   protected Uri ChatEndPoint => _settingsManager.Get<TextGenerationSettings>().ChatEndPoint;
   protected Uri ChatStreamedEndPoint => _settingsManager.Get<TextGenerationSettings>().StreamedChatEndPoint;
   public string DefaultModel => _settingsManager.Get<TextGenerationSettings>().DefaultModel;
   public TextGenerationParameters DefaultParameters => _settingsManager.Get<TextGenerationSettings>().DefaultParameters;


   public string Name => NAME;
   public string Description => DESCRIPTION;
   public IFeatureCollection Features => new FeatureCollection();

   private HttpClient _client = default!;
   private readonly JsonSerializerOptions _serializerOptions;

   public TextGenerationConnector(ILogger<TextGenerationConnector> logger, IPluginSettingsManager settingsManager)
   {
      _logger = logger;
      _settingsManager = settingsManager;

      _serializerOptions = new JsonSerializerOptions()
      {
         DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
      };

      _settingsManager.OnSettingsUpdated += OnSettingsUpdated;
      ApplySettings();
   }

   private void ApplySettings()
   {
      // we can't modify a HttpClient once it's created, so we need to dispose it and create a new one
      _client?.Dispose();
      _client = new HttpClient
      {
         Timeout = TimeSpan.FromMilliseconds(_settingsManager.Get<TextGenerationSettings>().Timeout)
      };
   }

   /// <summary>
   /// If the settings are updated, we need to update the client.
   /// </summary>
   /// <param name="sender"></param>
   /// <param name="settingType"></param>
   private void OnSettingsUpdated(object? sender, Type settingType)
   {
      if (settingType == typeof(TextGenerationSettings))
      {
         ApplySettings();
      }
   }

   public TFeatureType? GetFeature<TFeatureType>() => Features.Get<TFeatureType>();
   public void SetFeature<TFeatureType>(TFeatureType? feature) => Features.Set(feature);

   public async Task<IConversationalResponse?> RequestChatCompletionAsync(IConversationalRequest request)
   {
      ChatCompletionRequest apiRequest = BuildChatCompletionRequest(request, false);

      _logger.LogDebug("Performing request ${apiRequest}", apiRequest.Prompt);
      var sw = Stopwatch.StartNew();

      try
      {
         JsonContent content = JsonContent.Create(apiRequest, mediaType: null, null);
         // oobabooga TextGeneration API implementation requires content-lenght
         await content.LoadIntoBufferAsync().ConfigureAwait(false);
         using HttpResponseMessage response = await _client.PostAsync(ChatEndPoint, content, CancellationToken.None).ConfigureAwait(false);
         _logger.LogDebug("Request completed: {Request}", await response.RequestMessage!.Content!.ReadAsStringAsync().ConfigureAwait(false));

         if (response.IsSuccessStatusCode)
         {
            _logger.LogDebug("Request succeeded: {ResponseContent}", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            var responseData = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>().ConfigureAwait(false);

            sw.Stop();
            return new ConversationalResponse
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
            _logger.LogError("Request failed: {Error}", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
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

   public async IAsyncEnumerable<IConversationalStreamedResponse> RequestChatCompletionAsStreamAsync(IConversationalRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      // stream is not supported yet, fallback to normal request
      var response = await RequestChatCompletionAsync(request).ConfigureAwait(false);

      //using (ClientWebSocket webSocket = new ClientWebSocket())
      //{
      //   await webSocket.ConnectAsync(ChatStreamedEndPoint, cancellationToken).ConfigureAwait(false);

      //   byte[] receiveBuffer = new byte[1024];
      //   while (webSocket.State == WebSocketState.Open)
      //   {
      //      var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), cancellationToken).ConfigureAwait(false);
      //      if (result.MessageType == WebSocketMessageType.Text)
      //      {
      //         string receivedMessage = System.Text.Encoding.UTF8.GetString(receiveBuffer, 0, result.Count);
      //         yield return receivedMessage;
      //      }
      //   }
      //}

      if (response is null) yield break;

      yield return new ConversationalStreamedResponse
      {
         GeneratedMessage = response.GeneratedMessage,
         PromptTokens = response.PromptTokens,
         CumulativeTotalTokens = response.TotalTokens,
         CumulativeCompletionTokens = response.CompletionTokens,
         CumulativeResponseTime = response.ResponseTime
      };

      //ChatCompletionRequest apiRequest = BuildChatCompletionRequest(request, true);
      //_logger.LogDebug("Performing request ${apiRequest}", apiRequest.Messages);

      //var sw = Stopwatch.StartNew();

      //using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, EndPoint)
      //{
      //   Content = JsonContent.Create(apiRequest, null, _serializerOptions)
      //};
      //httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

      //using var httpResponseMessage = await _client.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

      //var stream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
      //await using var streamScope = stream.ConfigureAwait(false);
      //using var streamReader = new StreamReader(stream);
      //while (streamReader.EndOfStream is false)
      //{
      //   cancellationToken.ThrowIfCancellationRequested();
      //   var line = (await streamReader.ReadLineAsync(cancellationToken).ConfigureAwait(false))!;

      //   if (line.StartsWith(STREAM_DATA_MARKER))
      //      line = line[_streamDataMarkerLength..];

      //   if (string.IsNullOrWhiteSpace(line)) continue; //empty line

      //   if (line == "[DONE]") break;

      //   ChatCompletionResponse? streamedResponse = null;
      //   try
      //   {
      //      streamedResponse = JsonSerializer.Deserialize<ChatCompletionResponse>(line);
      //   }
      //   catch (Exception)
      //   {
      //      // if we can't deserialize the response, it's probably because it's an error, try to deserialize
      //      // the rest of the stream as an error message
      //      line += await streamReader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
      //      var error = JsonSerializer.Deserialize<ErrorResponse>(line);
      //      if (error?.Error is not null)
      //         throw new Exception($"Request failed with status code {error.Error.Code}: {error.Error.Message}");
      //   }

      //   if (streamedResponse is not null)
      //   {
      //      yield return new ConversationalStreamedResponse
      //      {
      //         GeneratedMessage = streamedResponse?.Choices.FirstOrDefault()?.Message?.Content,
      //         PromptTokens = streamedResponse?.Usage?.PromptTokens,
      //         CumulativeTotalTokens = streamedResponse?.Usage?.TotalTokens,
      //         CumulativeCompletionTokens = streamedResponse?.Usage?.CompletionTokens,
      //         CumulativeResponseTime = sw.Elapsed
      //      };
      //   }
      //}
   }

   /// <summary>
   /// Builds a <see cref="ChatCompletionRequest"/> from a <see cref="ChatApiRequest"/>.
   /// </summary>
   /// <param name="request">The <see cref="ChatApiRequest"/> to build from.</param>
   /// <returns>The built <see cref="ChatCompletionRequest"/>.</returns>
   private ChatCompletionRequest BuildChatCompletionRequest(IConversationalRequest request, bool requireStream)
   {
      var defaultParameters = DefaultParameters;

      var chatRequest = new ChatCompletionRequest(defaultParameters);

      //TopASamplings ignored
      //if(request.TopASamplings != null) { }
      if (request.MaxGeneratedTokens != null) { chatRequest.MaxNewTokens = request.MaxGeneratedTokens; }
      if (request.RepetitionPenality != null) { chatRequest.RepetitionPenalty = (double?)request.RepetitionPenality; }
      if (request.RepetitionPenalityRange != null) { chatRequest.EncoderRepetitionPenalty = (double?)request.RepetitionPenalityRange; }
      if (request.StopSequences != null) { chatRequest.StoppingStrings = request.StopSequences; }
      if (request.Temperature != null) { chatRequest.Temperature = (double?)request.Temperature; }
      if (request.TopKSamplings != null) { chatRequest.TopK = (int?)request.TopKSamplings; }
      if (request.TopPSamplings != null) { chatRequest.TopP = (double?)request.TopPSamplings; }
      if (request.TypicalSampling != null) { chatRequest.TypicalP = (double?)request.TypicalSampling; }
      // ignored properties by TextGeneration API
      // request.CompletionResults
      // request.ContextSize
      // request.LogitBias
      // request.ModelId
      // request.TailFreeSampling
      // request.TopASamplings
      // request.UserId

      // we need now to build the prompt since textgeneration api has just a single prompt field, not a list of messages with roles
      var sb = new StringBuilder();
      foreach (var message in request.Messages)
      {
         sb.AppendLine($"\n{message.Role}: {message.Content}"); // we add a new line to space more the messages belonging to different roles, not sure if it's needed
      }
      sb.AppendLine(ASSISTANT_ROLE_PREFIX); //append already the assistant role, so the completion will start from here and we can remove it later

      chatRequest.Prompt = sb.ToString();

      return chatRequest;
   }

   public void Dispose()
   {
      _settingsManager.OnSettingsUpdated -= OnSettingsUpdated;
   }
}

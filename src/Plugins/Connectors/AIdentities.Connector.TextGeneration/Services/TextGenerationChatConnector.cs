﻿using System.Diagnostics;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using AIdentities.Shared.Features.AIdentities.Models;
using AIdentities.Shared.Features.Core.Services;
using Microsoft.AspNetCore.Http.Features;

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
   private readonly JsonSerializerOptions _serializerOptions;

   public TextGenerationChatConnector(ILogger<TextGenerationChatConnector> logger, IPluginSettingsManager settingsManager)
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

   public async Task<IConversationalResponse?> RequestChatCompletionAsync(IConversationalRequest request, CancellationToken cancellationToken)
   {
      ChatCompletionRequest apiRequest = BuildChatCompletionRequest(request, false);

      _logger.DumpAsJson("Performing request", apiRequest);
      var sw = Stopwatch.StartNew();

      try
      {
         JsonContent content = JsonContent.Create(apiRequest, mediaType: null, null);
         // oobabooga TextGeneration API implementation requires content-lenght
         await content.LoadIntoBufferAsync().ConfigureAwait(false);
         using HttpResponseMessage response = await _client.PostAsync(ChatEndPoint, content, cancellationToken).ConfigureAwait(false);

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

   static string GetResponsePrefix(AIdentity aIdentity) => $"### {aIdentity.Name}: ";

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

   public async IAsyncEnumerable<IConversationalStreamedResponse> RequestChatCompletionAsStreamAsync(IConversationalRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      // stream is not supported yet, fallback to normal request
      var response = await RequestChatCompletionAsync(request, cancellationToken).ConfigureAwait(false);

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

      yield return new DefaultConversationalStreamedResponse
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

      // we need now to build the prompt since textgeneration api has just a single prompt field, not a list of messages with roles
      var sb = new StringBuilder();
      foreach (var message in request.Messages)
      {
         // we add a new line to space more the messages belonging to different roles, not sure if it's needed
         if (message.Role == DefaultConversationalRole.System)
         {
            sb.AppendLine($"INSTRUCTION: {message.Content}");
         }
         else
         {
            sb.AppendLine($"### {message.Name}: {message.Content}");
         }
      }
      sb.AppendLine(GetResponsePrefix(request.AIdentity)); //append already the assistant role, so the completion will start from here and we can remove it later

      var chatRequest = new ChatCompletionRequest(
         prompt: sb.ToString(),
         parameters: defaultParameters
         );

      //TopASamplings ignored
      //if(request.TopASamplings != null) { }
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
      return chatRequest;
   }

   public void Dispose()
   {
      _settingsManager.OnSettingsUpdated -= OnSettingsUpdated;
   }
}

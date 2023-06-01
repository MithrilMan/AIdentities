﻿using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using AIdentities.Connector.OpenAI.Models;
using AIdentities.Shared.Features.Core.Services;
using Microsoft.AspNetCore.Http.Features;
using MudBlazor.Charts;
using Polly;
using Polly.Retry;

namespace AIdentities.Connector.OpenAI.Services;
public class OpenAIChatConnector : IConversationalConnector, IDisposable
{
   const string NAME = nameof(OpenAIChatConnector);
   const string DESCRIPTION = "OpenAI Chat Connector that uses ChatCompletion API.";
   /// <summary>
   /// marker of the starting streamed data.
   /// </summary>
   const string STREAM_DATA_MARKER = "data: ";
   static readonly int _streamDataMarkerLength = STREAM_DATA_MARKER.Length;

   readonly ILogger<OpenAIChatConnector> _logger;
   readonly IPluginSettingsManager _settingsManager;

   public bool Enabled => _settingsManager.Get<OpenAISettings>().Enabled;
   public string Name => NAME;
   public string Description => DESCRIPTION;
   public IFeatureCollection Features => new FeatureCollection();

   protected Uri EndPoint => _settingsManager.Get<OpenAISettings>().ChatEndPoint;
   protected string DefaultModel => _settingsManager.Get<OpenAISettings>().DefaultChatModel;

   private HttpClient _client = default!;
   readonly AsyncRetryPolicy _retryPolicy;
   private readonly JsonSerializerOptions _serializerOptions;
   private readonly JsonSerializerOptions _debuggingSerializerOptions;

   public OpenAIChatConnector(ILogger<OpenAIChatConnector> logger, IPluginSettingsManager settingsManager)
   {
      _logger = logger;
      _settingsManager = settingsManager;

      _serializerOptions = new JsonSerializerOptions()
      {
         DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
      };

      _debuggingSerializerOptions = new JsonSerializerOptions()
      {
         DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
         WriteIndented = true
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

   public async Task<IConversationalResponse?> RequestChatCompletionAsync(IConversationalRequest request, CancellationToken cancellationToken)
   {
      ChatCompletionRequest apiRequest = BuildChatCompletionRequest(request, false);

      _logger.LogDebug("Performing request {apiRequest}", JsonSerializer.Serialize(apiRequest.Messages, _debuggingSerializerOptions));
      var sw = Stopwatch.StartNew();

      using HttpResponseMessage response = await _retryPolicy.ExecuteAsync(async () =>
      {
         return await _client.PostAsJsonAsync(EndPoint, apiRequest, _serializerOptions, cancellationToken: cancellationToken).ConfigureAwait(false);
      }).ConfigureAwait(false);

      _logger.LogDebug("Request completed: {Response}", await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));

      if (response.IsSuccessStatusCode)
      {
         var responseData = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);

         sw.Stop();
         return new DefaultConversationalResponse
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
         _logger.LogError("Request failed: {Error}", response.StatusCode);
         throw new Exception($"Request failed with status code {response.StatusCode}");
      }
   }

   public async IAsyncEnumerable<IConversationalStreamedResponse> RequestChatCompletionAsStreamAsync(IConversationalRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      ChatCompletionRequest apiRequest = BuildChatCompletionRequest(request, true);
      _logger.LogDebug("Performing stream request {apiRequest}", JsonSerializer.Serialize(apiRequest.Messages, _debuggingSerializerOptions));

      using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, EndPoint)
      {
         Content = JsonContent.Create(apiRequest, null, _serializerOptions)
      };
      httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

      var res = await _retryPolicy.ExecuteAsync(async () =>
      {
         await Task.CompletedTask.ConfigureAwait(false);
         try
         {
            return ConsumeStream(httpRequestMessage, cancellationToken);
         }
         catch (Exception ex)
         {
            _logger.LogError(ex, "Error while consuming stream");
            return AsyncEnumerable.Empty<IConversationalStreamedResponse>();
         }
      }).ConfigureAwait(false);

      await foreach (var item in res.ConfigureAwait(false))
      {
         yield return item;
      }
   }

   private async IAsyncEnumerable<IConversationalStreamedResponse> ConsumeStream(
      HttpRequestMessage httpRequestMessage,
      [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      var sw = Stopwatch.StartNew();

      var response = await _client.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
      using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
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
         catch (Exception ex)
         {
            _logger.LogError(ex, "Failed to deserialize response: {Response}", line);
            // if we can't deserialize the response, it's probably because it's an error, try to deserialize
            // the rest of the stream as an error message
            line += await streamReader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            var error = JsonSerializer.Deserialize<ErrorResponse>(line);
            if (error?.Error is not null)
               throw new Exception($"Request failed with status code {error.Error.Code}: {error.Error.Message}");
         }

         if (streamedResponse is not null)
         {
            yield return new DefaultConversationalStreamedResponse
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

   /// <summary>
   /// Builds a <see cref="ChatCompletionRequest"/> from a <see cref="IConversationalRequest"/>.
   /// </summary>
   /// <param name="request">The <see cref="ChatApiRequest"/> to build from.</param>
   /// <returns>The built <see cref="ChatCompletionRequest"/>.</returns>
   private ChatCompletionRequest BuildChatCompletionRequest(IConversationalRequest request, bool requireStream)
   {
      //measure request token size
      try
      {
         var tokenizer = SharpToken.GptEncoding.GetEncodingForModel(request.ModelId ?? DefaultModel);
         var tokenCount = tokenizer.Encode(string.Join('\n', request.Messages.Select(m => m.Content))).Count;
         _logger.LogDebug("Request token size: approx {TokenCount}", tokenCount);
      }
      catch { _logger.LogDebug("Request token size: unknown"); }

      return new ChatCompletionRequest
      {
         FrequencyPenalty = request.RepetitionPenalityRange,
         MaxTokens = request.MaxGeneratedTokens,
         Messages = request.Messages.Select(m => new ChatCompletionRequestMessage
         {
            Content = m.Content,
            Name = SanitizeName(m.Name),
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
   }

   /// <summary>
   /// Sanitizes a name to be used in the request.
   /// OpenAI's API doesn't allow names to contain spaces, so we replace them with underscores.
   /// Accepted characters are letters, digits, underscores, and hyphens.
   /// Regex pattern: ^[a-zA-Z0-9_-]{1,64}
   /// </summary>
   /// <param name="name"></param>
   /// <returns></returns>
   /// <exception cref="NotImplementedException"></exception>
   private static string? SanitizeName(string? name)
   {
      if (name is null) return null;

      //trim the name if too long
      if (name.Length > 64)
      {
         name = name[..64];
      }

      string pattern = @"[^a-zA-Z0-9_-]"; // matches any character that is not a letter, digit, underscore, or hyphen
      string replacement = "_";
      string sanitized = Regex.Replace(name, pattern, replacement);

      return sanitized;
   }

   private static ChatCompletionRoleEnum? MapRole(DefaultConversationalRole role) => role switch
   {
      DefaultConversationalRole.User => ChatCompletionRoleEnum.User,
      DefaultConversationalRole.Assistant => ChatCompletionRoleEnum.Assistant,
      DefaultConversationalRole.System => ChatCompletionRoleEnum.System,
      _ => throw new NotImplementedException()
   };

   public void Dispose()
   {
      _settingsManager.OnSettingsUpdated -= OnSettingsUpdated;
   }
}

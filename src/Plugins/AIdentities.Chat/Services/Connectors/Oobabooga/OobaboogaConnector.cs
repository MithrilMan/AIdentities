﻿using System.Net.Http.Json;
using AIdentities.Chat.Extendability;
using AIdentities.Chat.Services.Connectors.Oobabooga.API;
using AIdentities.Chat.Services.Connectors.OpenAI;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;

namespace AIdentities.Chat.Services.Connectors.Oobabooga;
public class OobaboogaConnector : IChatConnector
{
   const string NAME = nameof(OobaboogaConnector);
   const string DESCRIPTION = "Oobabooga Chat Connector that uses ChatCompletion API.";
   readonly ILogger<OobaboogaConnector> _logger;
   readonly IOptions<OobaboogaOptions> _options;
   readonly Uri _endpoint;

   public Uri Endpoint => _endpoint;
   public string Name => NAME;
   public string Description => DESCRIPTION;
   public IFeatureCollection Features => new FeatureCollection();

   private readonly HttpClient _client;
   private readonly JsonSerializerOptions _serializerOptions;

   public OobaboogaConnector(ILogger<OobaboogaConnector> logger, IOptions<OobaboogaOptions> options)
   {
      _logger = logger;
      _options = options;

      Uri.TryCreate(_options.Value.EndPoint, UriKind.Absolute, out _endpoint!);

      _client = CreateHttpClient();
      _serializerOptions = new JsonSerializerOptions()
      {
         DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
      };
   }

   public TFeatureType? GetFeature<TFeatureType>() => Features.Get<TFeatureType>();
   public void SetFeature<TFeatureType>(TFeatureType? feature) => Features.Set(feature);

   public async Task<ChatApiResponse?> SendMessageAsync(ChatApiRequest request)
   {
      throw new NotImplementedException();
      //var apiRequest = new ChatCompletionRequest
      //{
      //   FrequencyPenalty = request.RepetitionPenalityRange,
      //   MaxTokens = request.MaxGeneratedTokens,
      //   Messages = request.Messages.Select(m => new ChatCompletionRequestMessage
      //   {
      //      Content = m.Content,
      //      Name = m.Name,
      //      Role = MapRole(m.Role)
      //   }).ToList(),
      //   Model = request.ModelId ?? _options.Value.DefaultModel,
      //   PresencePenalty = request.RepetitionPenality,
      //   N = request.CompletitionResults,
      //   Stop = request.StopSequences,
      //   Stream = request.Stream,
      //   Temperature = request.Temperature,
      //   TopP = request.TopPSamplings,
      //   User = request.UserId,
      //};

      //_logger.LogDebug("Performing request ${apiRequest}", apiRequest.Messages);
      //using HttpResponseMessage response = await _client.PostAsJsonAsync(Endpoint, apiRequest, _serializerOptions).ConfigureAwait(false);
      //_logger.LogDebug("Request completed: {Request}", await response.RequestMessage!.Content!.ReadAsStringAsync().ConfigureAwait(false));

      //if (response.IsSuccessStatusCode)
      //{
      //   _logger.LogDebug("Request succeeded: {RequestContent}", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
      //   var responseData = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>().ConfigureAwait(false);
      //   return new ChatApiResponse
      //   {
      //      GeneratedMessage = responseData?.Choices.FirstOrDefault()?.Message.Content
      //   };
      //}
      //else
      //{
      //   _logger.LogError("Request failed: {Error}", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
      //   throw new Exception($"Request failed with status code {response.StatusCode}");
      //}
   }

   //private static ChatCompletionRoleEnum? MapRole(ChatApiRequest.MessageRole role) => role switch
   //{
   //   ChatApiRequest.MessageRole.User => ChatCompletionRoleEnum.User,
   //   ChatApiRequest.MessageRole.Assistant => ChatCompletionRoleEnum.Assistant,
   //   ChatApiRequest.MessageRole.System => ChatCompletionRoleEnum.System,
   //   _ => throw new NotImplementedException()
   //};

   //private async Task PerformStreamRequestAsync(ChatApiRequest request, CancellationToken cancellationToken)
   //{
   //   HttpClient client = CreateHttpClient();
   //   bool requestEnded = false;

   //   while (!cancellationToken.IsCancellationRequested && !requestEnded)
   //   {
   //      try
   //      {
   //         _logger.LogDebug("Establishing connection");
   //         var stream = await client.GetStreamAsync(Endpoint, cancellationToken).ConfigureAwait(false);
   //         using var streamReader = new StreamReader(stream);
   //         while (!streamReader.EndOfStream)
   //         {
   //            var message = await streamReader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
   //            _logger.LogDebug("Received a message");
   //            if (message == "DONE") // data: DONE
   //            {
   //               requestEnded = true;
   //            }
   //         }
   //      }
   //      catch (Exception ex)
   //      {
   //         _logger.LogError(ex, "Streaming failed.");
   //      }
   //   }
   //}

   private HttpClient CreateHttpClient()
   {
      HttpClient client = new HttpClient
      {
         Timeout = TimeSpan.FromMilliseconds(_options.Value.Timeout)
      };


      return client;
   }
}
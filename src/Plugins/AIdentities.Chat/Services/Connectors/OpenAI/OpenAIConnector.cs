using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using AIdentities.Chat.Extendability;
using AIdentities.Chat.Services.Connectors.OpenAI.API;
using AIdentities.Shared.Plugins.Connectors.Conversational;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;

namespace AIdentities.Chat.Services.Connectors.OpenAI;
public class OpenAIConnector : IConversationalConnector
{
   const string NAME = nameof(OpenAIConnector);
   const string DESCRIPTION = "OpenAI Chat Connector that uses ChatCompletion API.";
   /// <summary>
   /// marker of the starting streamed data.
   /// </summary>
   const string STREAM_DATA_MARKER = "data: ";
   static readonly int _streamDataMarkerLength = STREAM_DATA_MARKER.Length;

   readonly ILogger<OpenAIConnector> _logger;
   readonly IOptions<OpenAIOptions> _options;
   readonly Uri _endpoint;

   public Uri Endpoint => _endpoint;
   public string Name => NAME;
   public string Description => DESCRIPTION;
   public IFeatureCollection Features => new FeatureCollection();

   private readonly HttpClient _client;
   private readonly JsonSerializerOptions _serializerOptions;

   public OpenAIConnector(ILogger<OpenAIConnector> logger, IOptions<OpenAIOptions> options)
   {
      _logger = logger;
      _options = options;

      Uri.TryCreate(_options.Value.EndPoint, UriKind.Absolute, out _endpoint!);

      _client = CreateHttpClient();
      _serializerOptions = new JsonSerializerOptions()
      {
         DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
      };
   }

   public TFeatureType? GetFeature<TFeatureType>() => Features.Get<TFeatureType>();
   public void SetFeature<TFeatureType>(TFeatureType? feature) => Features.Set(feature);

   public async Task<IConversationalResponse?> RequestChatCompletionAsync(IConversationalRequest request)
   {
      ChatCompletionRequest apiRequest = BuildChatCompletionRequest(request, false);

      _logger.LogDebug("Performing request ${apiRequest}", apiRequest.Messages);
      Stopwatch sw = Stopwatch.StartNew();

      using HttpResponseMessage response = await _client.PostAsJsonAsync(_endpoint, apiRequest, _serializerOptions).ConfigureAwait(false);

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

      Stopwatch sw = Stopwatch.StartNew();

      using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _endpoint)
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
         {
            line = line[_streamDataMarkerLength..];
         }

         if (string.IsNullOrWhiteSpace(line))
         {
            _logger.LogDebug("Received EMPTY line");
            continue; //empty line
         }

         _logger.LogDebug("Received streamed response: {Response}", line);

         if (line == "[DONE]") break;

         ChatCompletionResponse? streamedResponse = JsonSerializer.Deserialize<ChatCompletionResponse>(line);

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
      HttpClient client = new HttpClient
      {
         Timeout = TimeSpan.FromMilliseconds(_options.Value.Timeout)
      };

      client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.Value.ApiKey);

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
      Model = request.ModelId ?? _options.Value.DefaultModel,
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
}

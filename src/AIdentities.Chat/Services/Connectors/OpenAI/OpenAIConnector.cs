using System.Net.Http.Json;
using AIdentities.Chat.Extendability;
using AIdentities.Chat.Services.Connectors.OpenAI.API;
using Microsoft.Extensions.Options;
using static AIdentities.Chat.Services.Connectors.OpenAI.API.ChatCompletionRequestMessage;

namespace AIdentities.Chat.Services.Connectors.OpenAI;
public class OpenAIConnector : IChatConnector
{
   const string NAME = nameof(OpenAIConnector);
   const string DESCRIPTION = "OpenAI Chat Connector that uses ChatCompletion API.";
   readonly ILogger<OpenAIConnector> _logger;
   readonly IOptions<OpenAIOptions> _options;
   readonly Uri _endpoint;

   public Uri Endpoint => _endpoint;
   public string Name => NAME;
   public string Description => DESCRIPTION;

   public IChatApiRequest Settings => throw new NotImplementedException();

   public OpenAIConnector(ILogger<OpenAIConnector> logger, IOptions<OpenAIOptions> options)
   {
      _logger = logger;
      _options = options;

      Uri.TryCreate(_options.Value.EndPoint, UriKind.Absolute, out _endpoint!);
   }

   public TFeatureType? GetFeature<TFeatureType>() => throw new NotImplementedException();
   public void SetFeature<TFeatureType>(TFeatureType? feature) => throw new NotImplementedException();

   public async Task<ChatApiResponse?> Request(ChatApiRequest request)
   {
      HttpClient client = CreateHttpClient();

      var apiRequest = new ChatCompletionRequest
      {
         FrequencyPenalty = request.RepetitionPenalityRange,
         MaxTokens = request.MaxGeneratedTokens,
         Messages = request.Messages.Select(m => new ChatCompletionRequestMessage
         {
            Content = m.Content,
            Name = m.Name,
            Role = Enum.Parse<ChatCompletionRoleEnum>(m.Role, true)
         }).ToList(),
         Model = request.ModelId,
         PresencePenalty = request.RepetitionPenality,
         N = request.CompletitionResults,
         Stop = request.StopSequences,
         Stream = request.Stream,
         Temperature = request.Temperature,
         TopP = request.TopPSamplings,
         User = request.UserId,
      };

      _logger.LogDebug("Performing request");
      using HttpResponseMessage response = await client.PostAsJsonAsync(Endpoint, apiRequest).ConfigureAwait(false);
      _logger.LogDebug("Request completed");

      if (response.IsSuccessStatusCode)
      {
         _logger.LogDebug("Request succeeded");
         return await response.Content.ReadFromJsonAsync<ChatApiResponse>().ConfigureAwait(false);
      }
      else
      {
         _logger.LogDebug("Request failed");
         throw new Exception($"Request failed with status code {response.StatusCode}");
      }
   }

   private async Task PerformStreamRequestAsync(ChatApiRequest request, CancellationToken cancellationToken)
   {
      HttpClient client = CreateHttpClient();
      bool requestEnded = false;

      while (!cancellationToken.IsCancellationRequested && !requestEnded)
      {
         try
         {
            _logger.LogDebug("Establishing connection");
            var stream = await client.GetStreamAsync(Endpoint, cancellationToken).ConfigureAwait(false);
            using var streamReader = new StreamReader(stream);
            while (!streamReader.EndOfStream)
            {
               var message = await streamReader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
               _logger.LogDebug("Received a message");
               if (message == "DONE") // data: DONE
               {
                  requestEnded = true;
               }
            }
         }
         catch (Exception ex)
         {
            _logger.LogError(ex, "Streaming failed.");
         }
      }
   }

   private HttpClient CreateHttpClient()
   {
      HttpClient client = new HttpClient
      {
         Timeout = TimeSpan.FromSeconds(5)
      };


      return client;
   }
}

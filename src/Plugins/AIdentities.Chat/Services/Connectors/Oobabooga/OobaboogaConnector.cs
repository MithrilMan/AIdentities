using System.Runtime.CompilerServices;
using AIdentities.Shared.Plugins.Connectors.Conversational;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;

namespace AIdentities.Chat.Services.Connectors.Oobabooga;
public class OobaboogaConnector : IConversationalConnector
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

   public Task<IConversationalResponse?> RequestChatCompletionAsync(IConversationalRequest request)
   {
      throw new NotImplementedException();
   }

   public IAsyncEnumerable<IConversationalStreamedResponse> RequestChatCompletionAsStreamAsync(IConversationalRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      throw new NotImplementedException();
   }


   private HttpClient CreateHttpClient()
   {
      HttpClient client = new HttpClient
      {
         Timeout = TimeSpan.FromMilliseconds(_options.Value.Timeout)
      };


      return client;
   }
}

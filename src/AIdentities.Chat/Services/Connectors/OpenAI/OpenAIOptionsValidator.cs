using AIdentities.Shared;

namespace AIdentities.Chat.Services.Connectors.OpenAI;

public class OpenAIOptionsValidator
{
   const string INVALID_ENDPOINT = $"Invalid {nameof(OpenAIOptions.EndPoint)} value, must be a valid Url";
   const string INVALID_API_KEY = $"Invalid {nameof(OpenAIOptions.ApiKey)} value, must be a valid OpenAI API Key";
   
   readonly ILogger<AppOptionsValidator> _logger;

   public OpenAIOptionsValidator(ILogger<AppOptionsValidator> logger)
   {
      _logger = logger;
   }

   public bool Validate(OpenAIOptions options)
   {
      _logger.LogInformation("Validating settings {OpenAIOptions}", JsonSerializer.Serialize(options));

      //ensure options.EndPoint is a valid Url that points to a valid http OpenAI API endpoint
      if (!Uri.TryCreate(options.EndPoint, UriKind.Absolute, out var uri)
         || !uri.IsWellFormedOriginalString()
         || !uri.IsAbsoluteUri
         || !uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase)
         || !uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
      {
         _logger.LogError(INVALID_ENDPOINT);
         return false;
      }

      //ensure ApiKey is not empty if the API are enabled
      if (options.Enabled && string.IsNullOrWhiteSpace(options.ApiKey))
      {
         _logger.LogError(INVALID_API_KEY);
         return false;
      }

      return true;
   }
}

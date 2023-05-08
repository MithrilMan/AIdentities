using Microsoft.Extensions.Options;

namespace AIdentities.Chat.Services.Connectors.Oobabooga;

public class OpenAIOptionsValidator : IValidateOptions<OobaboogaOptions>
{
   const string INVALID_ENDPOINT = $"Invalid {nameof(OobaboogaOptions.EndPoint)} value, must be a valid Url";
   const string INVALID_API_KEY = $"Invalid {nameof(OobaboogaOptions.ApiKey)} value, must be a valid OpenAI API Key";

   readonly ILogger<OpenAIOptionsValidator> _logger;

   public OpenAIOptionsValidator(ILogger<OpenAIOptionsValidator> logger)
   {
      _logger = logger;
   }

   public ValidateOptionsResult Validate(string? name, OobaboogaOptions options)
   {
      _logger.LogInformation("Validating settings {OpenAIOptions}", JsonSerializer.Serialize(options));

      if (!options.Enabled)
      {
         _logger.LogInformation("OpenAI API is disabled, skipping validation");
         return ValidateOptionsResult.Success;
      }

      // Ensure options.EndPoint is a valid Url that points to a valid http OpenAI API endpoint
      if (!Uri.TryCreate(options.EndPoint, UriKind.Absolute, out var uri)
          || !uri.IsWellFormedOriginalString()
          || !uri.IsAbsoluteUri
          || !(uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) || uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)))
      {
         return ValidateOptionsResult.Fail(INVALID_ENDPOINT);
      }

      // Ensure ApiKey is not empty if the API is enabled
      if (string.IsNullOrWhiteSpace(options.ApiKey))
         return ValidateOptionsResult.Fail(INVALID_API_KEY);

      return ValidateOptionsResult.Success;
   }
}

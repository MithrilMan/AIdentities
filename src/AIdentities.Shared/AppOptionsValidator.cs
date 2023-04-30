using System.ComponentModel.DataAnnotations;

namespace AIdentities.Shared;
public class AppOptionsValidator
{
   const string INVALID_CHARACTERS_IN_PACKAGEFOLDER = $"Invalid {nameof(AppOptions.PackageFolder)} value, contains invalid characters";
   const string INVALID_PLUGINS_FILES_EXTENSIONS_EMPTY = $"Invalid {nameof(AppOptions.AllowedPluginResourceExtensions)}. Cannot be null or empty.";
   const string INVALID_PLUGINS_FILES_EXTENSIONS = $"Invalid {nameof(AppOptions.AllowedPluginResourceExtensions)}";

   readonly ILogger<AppOptionsValidator> _logger;

   public AppOptionsValidator(ILogger<AppOptionsValidator> logger)
   {
      _logger = logger;
   }

   public bool Validate(AppOptions options)
   {
      _logger.LogInformation("Validating settings {AIdentitiesConfiguration}", JsonSerializer.Serialize(options));


      //check if it's a valid relative path
      if (!PathUtils.IsValidPath(options.PackageFolder))
      {
         _logger.LogCritical(INVALID_CHARACTERS_IN_PACKAGEFOLDER);
         throw new ValidationException(INVALID_CHARACTERS_IN_PACKAGEFOLDER);
      }

      if (options.AllowedPluginResourceExtensions?.Any() ?? false)
      {
         //convert the foreach to for
         for (int i = 0; i < options.AllowedPluginResourceExtensions.Length; i++)
         {
            var extension = options.AllowedPluginResourceExtensions[i];
            if (string.IsNullOrEmpty(extension))
            {
               _logger.LogCritical(INVALID_CHARACTERS_IN_PACKAGEFOLDER);
               throw new ValidationException(INVALID_PLUGINS_FILES_EXTENSIONS_EMPTY);
            }

            //sanitize extension
            options.AllowedPluginResourceExtensions[i] = extension.Trim().ToLower();

            if (!extension.StartsWith(".") || Path.GetInvalidFileNameChars().Any(extension.Contains))
            {
               _logger.LogCritical(INVALID_CHARACTERS_IN_PACKAGEFOLDER);
               throw new ValidationException(INVALID_PLUGINS_FILES_EXTENSIONS);
            }
         }
      }
      else
      {
         _logger.LogWarning($"{nameof(AppOptions.AllowedPluginResourceExtensions)} is empty, all plugin files will be copied");
      }

      return true;
   }
}

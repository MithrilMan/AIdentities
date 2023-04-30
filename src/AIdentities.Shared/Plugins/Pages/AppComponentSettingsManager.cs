using Microsoft.Extensions.Options;

namespace AIdentities.Shared.Plugins.Pages;

public class AppComponentSettingsManager : IAppComponentSettingsManager
{
   private const string SETTINGS_FILE = "aid-settings.json";
   readonly ILogger<AppComponentSettingsManager> _logger;
   private readonly AppOptions _options;
   readonly Dictionary<string, object> _settings;

   public AppComponentSettingsManager(ILogger<AppComponentSettingsManager> logger, IOptions<AppOptions> options)
   {
      _logger = logger;
      _options = options.Value;

      //var read json from root disk, containing all settings for all components using JsonDeserializer
      _settings = LoadSettings()!;
   }

   public TAppComponentSettings? GetSettings<TAppComponentSettings>(string settingsKey)
    where TAppComponentSettings : class, IAppComponentSettings, new()
   {
      if (settingsKey is null) throw new ArgumentNullException(nameof(settingsKey));

      var value = _settings.GetValueOrDefault(settingsKey);
      if (value is null)
      {
         _logger.LogDebug("Settings {SettingKey} not found, creating default.", settingsKey);
         return new TAppComponentSettings();
      }

      var settings = JsonSerializer.Serialize(value);
      _logger.LogDebug("GetAppComponentSettings {SettingKey}: {SettingValue}", settingsKey, settings);
      return JsonSerializer.Deserialize<TAppComponentSettings>(settings);
   }

   public bool SetSettings<TAppComponentSettings>(string settingsKey, TAppComponentSettings settings)
      where TAppComponentSettings : class, IAppComponentSettings, new()
   {
      if (settingsKey is null) throw new ArgumentNullException(nameof(settingsKey));

      _settings[settingsKey] = settings;

      return true;
   }


   private Dictionary<string, object>? LoadSettings()
   {
      try
      {
         if(!File.Exists(SETTINGS_FILE))
         {
            _logger.LogWarning("Settings file not found: {SettingsFile}", SETTINGS_FILE);
            return new();
         }

         var file = File.ReadAllText(SETTINGS_FILE);
         return JsonSerializer.Deserialize<Dictionary<string, object>>(file) ?? new();
      }
      catch (Exception ex)
      {
         _logger.LogError(ex, "Error loading settings file.");
         return new();
      }
   }

   public ValueTask<bool> SaveSettingsAsync(CancellationToken cancellationToken)
   {
      try
      {
         var jsonString = JsonSerializer.Serialize(_settings);
         File.WriteAllText(SETTINGS_FILE, jsonString);

         return ValueTask.FromResult(true);
      }
      catch (Exception ex)
      {
         const string message = "Error saving settings file.";
         _logger.LogError(ex, message);

         return ValueTask.FromResult(false);
      }
   }
}

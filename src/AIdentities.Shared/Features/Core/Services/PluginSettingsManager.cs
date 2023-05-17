using AIdentities.Shared.Features.Core.Abstracts;
using AIdentities.Shared.Plugins.Storage;

namespace AIdentities.Shared.Features.Core.Services;

public class PluginSettingsManager : IPluginSettingsManager
{
   readonly ILogger<PluginSettingsManager> _logger;
   private readonly Dictionary<Type, PluginSettingRegistration> _pluginSettingsRegistrations;
   private readonly Dictionary<Type, IPluginSettings> _pluginSettings = new();

   public PluginSettingsManager(ILogger<PluginSettingsManager> logger, IEnumerable<PluginSettingRegistration> pluginSettingsRegistrations)
   {
      _logger = logger;
      _pluginSettingsRegistrations = pluginSettingsRegistrations.ToDictionary(r => r.PluginSettingType, r => r);
   }

   public event EventHandler<IPluginSettings>? OnSettingsUpdated;

   public TPluginSettings Get<TPluginSettings>()
      where TPluginSettings : class, IPluginSettings, new()
      => (TPluginSettings)GetAsync(typeof(TPluginSettings)).Result;

   public async ValueTask<TPluginSettings> GetAsync<TPluginSettings>()
      where TPluginSettings : class, IPluginSettings, new()
      => (TPluginSettings)await GetAsync(typeof(TPluginSettings)).ConfigureAwait(false);

   public async ValueTask<IPluginSettings> GetAsync(Type pluginSettingsType)
   {
      if (_pluginSettings.TryGetValue(pluginSettingsType, out var pluginSettings))
      {
         return pluginSettings;
      }

      // try to load the settings from the plugin storage
      if (!_pluginSettingsRegistrations.TryGetValue(pluginSettingsType, out var registration))
      {
         _logger.LogError("Plugin settings for {PluginSettingsType} not found.", pluginSettingsType.FullName);
         throw new InvalidOperationException($"Plugin settings for {pluginSettingsType.FullName} not found. You have to register it as PluginSettingsRegistration.");
      }

      var settings = await registration.PluginStorage.LoadSettingsAsync(pluginSettingsType, (IPluginSettings)Activator.CreateInstance(pluginSettingsType)!).ConfigureAwait(false);
      _pluginSettings[pluginSettingsType] = settings;

      return settings;
   }

   public async ValueTask SetAsync<TPluginSettings>(TPluginSettings pluginSettings)
      where TPluginSettings : class, IPluginSettings, new()
   {
      await SetAsync(typeof(TPluginSettings), pluginSettings).ConfigureAwait(false);
   }

   public async ValueTask SetAsync(Type pluginSettingsType, IPluginSettings pluginSettings)
   {
      if (!_pluginSettingsRegistrations.TryGetValue(pluginSettingsType, out var registration))
      {
         _logger.LogError("Plugin settings for {PluginSettingsType} not found.", pluginSettingsType.FullName);
         throw new InvalidOperationException($"Plugin settings for {pluginSettingsType.FullName} not found. You have to register it as PluginSettingsRegistration.");
      }

      _pluginSettings[pluginSettingsType] = pluginSettings;

      await registration.PluginStorage.SaveSettingsAsync(pluginSettings).ConfigureAwait(false);

      OnSettingsUpdated?.Invoke(this, pluginSettings);
   }
}

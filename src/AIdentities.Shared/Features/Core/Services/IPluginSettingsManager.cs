using AIdentities.Shared.Features.Core.Abstracts;

namespace AIdentities.Shared.Features.Core.Services;

/// <summary>
/// Manage the Plugin Settings, keeping an updated reference to the current settings.
/// </summary>
public interface IPluginSettingsManager
{
   /// <summary>
   /// This event is raised when the settings are updated.
   /// </summary>
   event EventHandler<IPluginSettings> OnSettingsUpdated;

   /// <summary>
   /// Retrieve the specified plugin settings.
   /// </summary>
   /// <typeparam name="TPluginSettings">The type of the plugin settings.</typeparam>
   /// <returns>The plugin settings.
   /// If no plugin settings are found, a default instance is returned.
   /// </returns>
   public TPluginSettings Get<TPluginSettings>()
      where TPluginSettings : class, IPluginSettings, new();

   /// <summary>
   /// Retrieve the specified plugin settings.
   /// </summary>
   /// <typeparam name="TPluginSettings">The type of the plugin settings.</typeparam>
   /// <returns>The plugin settings.
   /// If no plugin settings are found, a default instance is returned.
   /// </returns>
   ValueTask<TPluginSettings> GetAsync<TPluginSettings>()
      where TPluginSettings : class, IPluginSettings, new();

   /// <summary>
   /// Retrieve the specified plugin settings.
   /// </summary>
   /// <returns>The plugin settings.
   /// If no plugin settings are found, a default instance is returned.
   /// </returns>
   ValueTask<IPluginSettings> GetAsync(Type pluginSettingsType);

   /// <summary>
   /// Set the plugin settings.
   /// Note that this method doesn't save the settings, it only updates the reference to the current settings.
   /// </summary>
   /// <typeparam name="TPluginSettings">The type of the plugin settings.</typeparam>
   /// <param name="pluginSettings">The plugin settings to set.</param>
   ValueTask SetAsync<TPluginSettings>(TPluginSettings pluginSettings)
      where TPluginSettings : class, IPluginSettings, new();

   /// <summary>
   /// Set the plugin settings.
   /// Note that this method doesn't save the settings, it only updates the reference to the current settings.
   /// </summary>
   /// <param name="pluginSettings">The plugin settings to set.</param>
   ValueTask SetAsync(Type pluginSettingsType, IPluginSettings pluginSettings);
}

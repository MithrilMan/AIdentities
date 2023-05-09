using AIdentities.Shared.Features.AIdentities.Services;
using AIdentities.Shared.Features.Core.Abstracts;
using AIdentities.Shared.Features.Core.Services;
using AIdentities.Shared.Plugins.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace AIdentities.Shared.Plugins;

/// <summary>
/// A base implementation of the plugin entry point.
/// It has to be implemented by the plugin and it is used to register the plugin services and declare its capabilities.
/// Contains some helper methods to register features.
/// </summary>
public abstract class BasePluginEntry : IPluginEntry
{
   protected PluginManifest _manifest = default!;
   protected IPluginStorage _storage = default!;
   private IServiceCollection _services = default!;

   void IPluginEntry.Initialize(PluginManifest manifest, IServiceCollection services, IPluginStorage pluginStorage)
   {
      _manifest = manifest;
      _storage = pluginStorage;
      _services = services;

      RegisterServices(services);
   }

   /// <summary>
   /// Registers a feature to be exposed in the AIdentity management page.
   /// </summary>
   /// <typeparam name="TFeature">The AIdentity feature type that will be saved along the AIdentity.</typeparam>
   /// <typeparam name="TFeatureTab">The AIdentity feature component Type that will be used to edit the AIdentity feature.</typeparam>
   /// <param name="uiTitle">The UI title that will be shown on the Tab Panel.</param>
   /// <exception cref="InvalidOperationException">Thrown if the plugin is not initialized.</exception>
   protected void RegisterFeature<TFeature, TFeatureTab>(string uiTitle)
     where TFeature : class, IAIdentityFeature
     where TFeatureTab : class, IAIdentityFeatureTab<TFeature>
   {
      if (_services == null) throw new InvalidOperationException("Cannot register a feature before the plugin is initialized.");

      // Register the AIdentity feature to expose an editor in the AIdentity management page.
      _services.AddSingleton(new AIdentityFeatureRegistration(typeof(TFeature), typeof(TFeatureTab), uiTitle));
   }

   /// <summary>
   /// Registers a plugin settings to be exposed in the Settings management page.
   /// </summary>
   /// <typeparam name="TPluginSettings">The plugin settings Type that will be saved into the ApplicationSettings.</typeparam>
   /// <typeparam name="TPluginSettingsTab">The plugin settings component Type that will be used to edit the plugin settings.</typeparam>
   /// <param name="uiTitle">The UI title that will be shown on the Tab Panel.</param>
   /// <exception cref="InvalidOperationException">Thrown if the plugin is not initialized.</exception>
   protected void RegisterPluginSettings<TPluginSettings, TPluginSettingsTab>(string uiTitle)
     where TPluginSettings : class, IPluginSettings
     where TPluginSettingsTab : class, IPluginSettingsTab<TPluginSettings>
   {
      if (_services == null) throw new InvalidOperationException("Cannot register a feature before the plugin is initialized.");

      // Register the AIdentity feature to expose an editor in the AIdentity management page.
      _services.AddSingleton(new PluginSettingRegistration(typeof(TPluginSettings), typeof(TPluginSettingsTab), uiTitle));
   }

   /// <summary>
   /// Has to be implemented by the plugin and it is used to register the plugin services and declare its capabilities.
   /// </summary>
   /// <param name="services"></param>
   public abstract void RegisterServices(IServiceCollection services);
}

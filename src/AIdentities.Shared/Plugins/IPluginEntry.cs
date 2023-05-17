using AIdentities.Shared.Plugins.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace AIdentities.Shared.Plugins;

/// <summary>
/// The plugin entry point.
/// It has to be implemented by the plugin and it is used to register the plugin services and declare its capabilities.
/// </summary>
public interface IPluginEntry
{
   /// <summary>
   /// Initializes the plugin service.
   /// This methods allow the plugin to register its services and declare its capabilities.
   /// When this method is called, its instance is created by a temporary service provider
   /// in order to allow the plugin to use the service collection to register its services.
   /// Once the method is completed, the temporary service provider is disposed and a new one
   /// will be created with proper DI resolution.
   /// </summary>
   /// <param name="manifest">The plugin manifest used to load the plugin.</param>
   /// <param name="services">The service collection to use to register the plugin services.</param>
   void Initialize(PluginManifest manifest, IServiceCollection services);
}

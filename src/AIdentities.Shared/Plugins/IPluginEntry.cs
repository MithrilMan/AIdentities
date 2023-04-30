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
   /// </summary>
   /// <param name="manifest">The plugin manifest used to load the plugin.</param>
   /// <param name="services">The service collection to use to register the plugin services.</param>
   /// <param name="pluginStorage">The plugin storage to use to deal with files.</param>
   void Initialize(PluginManifest manifest, IServiceCollection services, IPluginStorage pluginStorage);
}

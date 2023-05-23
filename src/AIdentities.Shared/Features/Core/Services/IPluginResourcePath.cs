using AIdentities.Shared.Plugins;

namespace AIdentities.Shared.Features.Core.Services;

/// <summary>
/// Helper class that allow to easily map relative plugin resources to the right path managed by blazor.
/// </summary>
public interface IPluginResourcePath
{
   /// <summary>
   /// Returns the path of a plugin resource based on the relative path from plugin wwwroot folder.
   /// </summary>
   /// <typeparam name="TPlugin">The plugin entry type used to identify the plugin.</typeparam>
   /// <param name="relativePath">The relative path from plugin wwwroot folder.</param>
   /// <returns>The path of the plugin resource.</returns>
   string GetContentPath<TPlugin>(string relativePath) where TPlugin : IPluginEntry;
}

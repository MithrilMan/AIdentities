namespace AIdentities.Shared.Plugins.Storage;

/// <summary>
/// Represents a factory for creating <see cref="IPluginStorage"/> instances.
/// </summary>
public interface IPluginStorageFactory
{
   /// <summary>
   /// Creates a new <see cref="IPluginStorage"/> instance for the given <paramref name="pluginManifest"/>.
   /// </summary>
   /// <typeparam name="TPluginEntry">The type of the plugin entry.</typeparam>
   /// <param name="pluginManifest">The plugin manifest.</param>
   /// <returns>A new <see cref="IPluginStorage"/> instance.</returns>
   public PluginStorage<TPluginEntry> CreatePluginStorage<TPluginEntry>(PluginManifest pluginManifest)
       where TPluginEntry : IPluginEntry;
}

namespace AIdentities.UI.Features.Core.Services.Plugins;

public interface IPluginFoldersProvider
{
   /// <summary>
   /// Gets the plugin package path.
   /// This path is the root of both the plugin storage and the plugin assets.
   /// </summary>
   /// <returns>The plugin package path.</returns>
   string GetPluginAssetsPath();

   /// <summary>
   /// Gets the plugin storage path.
   /// This path is the root of the plugin storage where all the plugin generated content should go.
   /// </summary>
   /// <returns>The plugin storage path.</returns>
   string GetPluginsStoragePath();

   /// <summary>
   /// Gets the plugin installation root.
   /// This folder contains the plugin assets extracted from the plugin package.
   /// </summary>
   /// <param name="pluginManifest"></param>
   /// <returns></returns>
   string GetPluginInstallationRoot(PluginManifest pluginManifest);

   /// <summary>
   /// Gets the plugin installation root.
   /// This folder contains the plugin assets extracted from the plugin package.
   /// This is an unsafe method because doesn't use the plugin manifest to build the path.
   /// </summary>
   /// <param name="pluginFullName">The plugin name.</param>
   /// <returns></returns>
   string GetPluginInstallationRoot(string pluginFullName);

   /// <summary>
   /// Gets the plugin enabled file path.
   /// This file contains the list of enabled plugins.
   /// </summary>
   /// <returns></returns>
   string GetPluginEnabledFilePath();
}

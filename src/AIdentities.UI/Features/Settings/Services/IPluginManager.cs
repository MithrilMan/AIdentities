using System.Reflection;
using Microsoft.AspNetCore.Components.Forms;

namespace AIdentities.UI.Features.Settings.Services;

public interface IPluginManager
{
   /// <summary>
   /// Returns a list of loaded packages.
   /// Package definitions are created when a package is loaded and contain all relevant information about the package
   /// like exported <see cref="IAppPage"/>s, <see cref="IConnector"/>s, assets, etc.
   /// </summary>
   IEnumerable<Package> LoadedPackages { get; }

   /// <summary>
   /// Returns a list of plugin packages that have been uploaded.
   /// </summary>
   IEnumerable<PluginStatus> StoredPlugins { get; }

   /// <summary>
   /// Returns a list of assemblies that contain components that can be used in the UI.
   /// </summary>
   IEnumerable<Assembly> PagePluginAssemblies { get; }

   /// <summary>
   /// Returns a list of invalid plugins.
   /// It's not always possible to obtain a PluginManifest for invalid plugins so 
   /// the keys are the full name of the package that corresponds to the folder name
   /// in the plugin folder.
   /// The value is the reason why the plugin is invalid.
   /// </summary>
   IReadOnlyDictionary<string, string> InvalidPlugins { get; }

   /// <summary>
   /// Event that is published when a package is loaded.
   /// </summary>
   event EventHandler<Package>? PackageLoaded;

   /// <summary>
   /// Tries to store the uploaded package file in the plugin folder.
   /// </summary>
   /// <param name="packageFile">The uploaded package file.</param>
   /// <returns>The plugin status of the uploaded package.</returns>
   ValueTask<PluginStatus> StorePackageAsync(IBrowserFile packageFile);

   /// <summary>
   /// Loads all the stored packages that are flagged as enabled.
   /// Once a package is loaded, a Package definition with all relevant information is added for each
   /// loaded package.
   /// If a package cannot be loaded, it is added to the <see cref="InvalidPlugins"/> list.
   /// For each loaded package, a <see cref="PackageLoaded"/> event is published.
   /// </summary>
   /// <param name="services">The service collection where plugin services will be registred into.</param>
   ValueTask LoadStoredPackagesAsync(IServiceCollection services);

   /// <summary>
   /// Tries to remove the plugin package.
   /// </summary>
   /// <param name="manifest">The manifest of the package to remove.</param>
   /// <returns>True if the package was removed, false otherwise.</returns>
   ValueTask<bool> RemovePackageAsync(PluginManifest manifest);

   /// <summary>
   /// Tries to remove the plugin package.
   /// </summary>
   /// <param name="packageName">The name of the package to remove.</param>
   /// <returns>True if the package was removed, false otherwise.</returns>
   ValueTask<bool> RemovePackageAsync(string packageName);

   /// <summary>
   /// Tries to enable the plugin package.
   /// In order to enable a package, the application has to be restarted.
   /// </summary>
   /// <param name="manifest">The manifest of the package to enable.</param>
   ValueTask ActivatePackageAsync(PluginManifest manifest);

   /// <summary>
   /// Tries to disable the plugin package.
   /// In order to disable a package, the application has to be restarted.
   /// </summary>
   /// <param name="manifest">The manifest of the package to disable.</param>
   ValueTask DisablePackageAsync(PluginManifest manifest);
}

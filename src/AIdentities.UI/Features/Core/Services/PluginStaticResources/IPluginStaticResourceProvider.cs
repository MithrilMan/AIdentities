using AIdentities.UI.Features.Core.Services.Plugins;
using Microsoft.Extensions.FileProviders;

namespace AIdentities.UI.Features.Core.Services.PluginStaticResources;

public interface IPluginStaticResourceProvider : IFileProvider
{
   /// <summary>
   /// Update the dependencies.
   /// When the app startup, this instance is created before the DI container is built.
   /// We need to inject the dependencies after the DI container is built in order
   /// to set the dependencies on the instances of the current container.
   /// </summary>
   /// <param name="logger"></param>
   /// <param name="pluginFoldersProvider"></param>
   void InjectDependencies(ILogger<IPluginStaticResourceProvider> logger, IPluginFoldersProvider pluginFoldersProvider);

   /// <summary>
   /// After the dependencies have been injected, we can initialize the plugin static resource provider.
   /// </summary>
   /// <param name="loadedPackages"></param>
   void Initialize(IEnumerable<Package> loadedPackages);
}

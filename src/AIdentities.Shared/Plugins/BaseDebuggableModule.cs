using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AIdentities.Shared.Plugins;

/// <summary>
/// This class is an helper to create a debuggable module.
/// Prevents the need to create boilerplate for the module registration.
/// </summary>
/// <typeparam name="TPluginEntry"></typeparam>
public abstract class BaseDebuggableModule<TPluginEntry> : IDebuggableModule
   where TPluginEntry : IPluginEntry, new()
{
   public virtual string Name => GetType().Namespace!;
   public virtual string Description { get; } = $"Debuggable module for {typeof(TPluginEntry).Name}";
   public virtual string Version { get; } = "0.0.1-debuggableModule";
   public virtual string Author { get; } = "Debuggable Module Creator";

   void IDebuggableModule.RegisterServices(IServiceCollection services, IHostEnvironment hostEnvironment)
   {
      var plugin = new TPluginEntry();

      var manifest = new PluginManifest()
      {
         Signature = new()
         {
            Name = Name,
            Version = Version,
            Author = Author
         },
         Description = Description,
      };

      //var sp = services.BuildServiceProvider();
      //var pluginStorageFactory = sp.GetRequiredService<IPluginStorageFactory>();
      //var pluginStorage = pluginStorageFactory.CreatePluginStorage<TPluginEntry>(manifest);

      plugin.Initialize(manifest, services);

      // important, to expose the plugin assembly when referencing this plugin directly in the AIdentities project.
      services.AddSingleton(typeof(IDebuggableModule), GetType());
   }

   /// <summary>
   /// This method is meant to be used to register the services needed by the module.
   /// </summary>
   /// <param name="services">The service collection where to register the services.</param>
   /// <param name="hostEnvironment">The host environment.</param>
   protected abstract void RegisterServices(IServiceCollection services, IHostEnvironment hostEnvironment);
}

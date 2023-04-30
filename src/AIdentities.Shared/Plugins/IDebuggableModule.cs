using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AIdentities.Shared.Plugins;

/// <summary>
/// Marker interface for modules that can be debugged.
/// When you want to develop a module that can be debugged, instead of having to constantly update the plugin in the AIdentities host, you can
/// instead add a direct reference of your module to the AIdentities project and then register an interface of this type in your module.
/// A background worker is spawn in AIdentities when compiled in Debug mode and will automatically discover all modules that implement this interface and 
/// register their assembly in the list of assemblies that exposes a razor Page.
/// This will be used to dynamically load the razor pages in the UI and show them in the menu.
/// </summary>
public interface IDebuggableModule
{
   /// <summary>
   /// Register services for the module.
   /// </summary>
   /// <param name="services">The service collection where to register the services.</param>
   /// <param name="hostEnvironment">The host environment.</param>
   void RegisterServices(IServiceCollection services, IHostEnvironment hostEnvironment);
}

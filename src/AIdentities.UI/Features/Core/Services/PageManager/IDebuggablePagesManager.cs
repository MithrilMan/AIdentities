using System.Reflection;

namespace AIdentities.UI.Features.Core.Services.PageManager;

/// <summary>
/// Exposes a list of assemblies that contain components that can be used in the UI.
/// This is meant to be used when developing a AIdentities plugin because allow the developer to reference the 
/// plugin directly in the AIdentities.UI project and then register an <see cref="IDebuggableModule"/> to expose its assembly.
/// </summary>
public interface IDebuggablePagesManager
{
   /// <summary>
   /// A list of assemblies that contain an implementation of a IDebuggableModule and may contain
   /// Page routes that can be used by the AIdentities host to expose the pages on the UI.
   /// </summary>
   IEnumerable<Assembly> DebuggableModuleAssemblies { get; }

   /// <summary>
   /// Used to register the services needed by the module.
   /// This is meant to be invoked by an instance generated before the final service provider is built.
   /// </summary>
   /// <param name="services"></param>
   /// <param name="hostEnvironment"></param>
   void RegisterServices(IServiceCollection services, IHostEnvironment hostEnvironment);

   /// <summary>
   /// Search for pages in the debuggable module assembly.
   /// </summary>
   IEnumerable<PageDefinition> FindPageDefinitions();
}

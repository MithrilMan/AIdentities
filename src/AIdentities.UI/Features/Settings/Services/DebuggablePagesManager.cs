using System.Reflection;

namespace AIdentities.UI.Features.Settings.Services;

public class DebuggablePagesManager : IDebuggablePagesManager
{
   private readonly List<Assembly> _debuggableModuleAssemblies = new();
   readonly ILogger<DebuggablePagesManager> _logger;
   readonly IEnumerable<IDebuggableModule> _debuggableModules;
   readonly IPackageInspector _packageInspector;

   public IEnumerable<Assembly> DebuggableModuleAssemblies => _debuggableModuleAssemblies;

   public DebuggablePagesManager(ILogger<DebuggablePagesManager> logger, IEnumerable<IDebuggableModule> debuggableModules, IPackageInspector packageInspector)
   {
      _logger = logger;
      _debuggableModules = debuggableModules;
      _packageInspector = packageInspector;

      RegisterAssemblies();
   }

   /// <summary>
   /// Register all assemblies from debuggable modules.
   /// _debuggableModuleAssemblies is populated by registering into a DI the IDebuggableModule with.
   /// </summary>
   private void RegisterAssemblies()
   {
      foreach (var module in _debuggableModules)
      {
         var moduleType = module.GetType();
         _logger.LogDebug("Registering debuggable module {Module}", moduleType.FullName);
         _debuggableModuleAssemblies.Add(moduleType.Assembly);
      }
   }

   public void RegisterServices(IServiceCollection services, IHostEnvironment hostEnvironment)
   {
      foreach (var module in _debuggableModules)
      {
         _logger.LogDebug("Registering services for debuggable module {Module}", module.GetType().FullName);
         module.RegisterServices(services, hostEnvironment);
      }
   }

   public IEnumerable<PageDefinition> FindPageDefinitions()
   {
      List<PageDefinition> _pageDefinitions = new();
      foreach (var module in _debuggableModules)
      {
         var moduleType = module.GetType();
         _logger.LogDebug("Finding pages for debuggable module {Module}", moduleType.FullName);
         _pageDefinitions.AddRange(_packageInspector.FindPageDefinitions(moduleType.Assembly));
      }

      return _pageDefinitions;
   }
}

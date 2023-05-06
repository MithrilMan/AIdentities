using System.Reflection;

namespace AIdentities.UI.Features.Core.Services.Plugins;

public class PackageInspector : IPackageInspector
{
   readonly ILogger<PackageInspector> _logger;

   public PackageInspector(ILogger<PackageInspector> logger)
   {
      _logger = logger;
   }

   public IReadOnlyList<PageDefinition> FindPageDefinitions(Assembly pluginAssembly)
   {
      List<PageDefinition> pageDefinitions = new();

      foreach (var type in pluginAssembly.GetTypes())
      {
         if (type.IsAssignableTo(typeof(IAppPage)))
         {
            _logger.LogTrace("Found page {PageType}", type.FullName);

            if (type.GetCustomAttribute<PageDefinitionAttribute>() is not PageDefinitionAttribute pageDefinitionAttribute)
            {
               _logger.LogWarning("PageDefinitionAttribute not found on {PageType}. Not Valid.", type.FullName);
               continue;
            }

            var pageDefinition = pageDefinitionAttribute.GetPageDefinition();

            _logger.LogTrace("{PageType} is a valid page. Assembly {AssemblyName} will be used to scaffold for page urls.", type.FullName, type.Assembly.FullName);

            pageDefinitions.Add(pageDefinition);
         }
      }

      return pageDefinitions;
   }

   public IReadOnlyList<IConnector> FindConnectors(Assembly pluginAssembly)
   {
      //TODO
      return new List<IConnector>();
   }
}

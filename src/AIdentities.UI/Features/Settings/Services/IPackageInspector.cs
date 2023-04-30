using System.Reflection;

namespace AIdentities.UI.Features.Settings.Services;
public interface IPackageInspector
{
   /// <summary>
   /// Searches the <see cref="PageDefinition"/> of all types that implements IAppPage properly
   /// </summary>
   /// <param name="pluginAssembly"></param>
   /// <returns></returns>
   IReadOnlyList<PageDefinition> FindPageDefinitions(Assembly pluginAssembly);

   IReadOnlyList<IConnector> FindConnectors(Assembly pluginAssembly);
}

using System.Reflection;

namespace AIdentities.UI.Features.Core.Services.Plugins;
public interface IPackageInspector
{
   /// <summary>
   /// Searches the <see cref="PageDefinition"/> of all types that implements IAppPage properly
   /// </summary>
   /// <param name="pluginAssembly"></param>
   /// <returns></returns>
   IReadOnlyList<PageDefinition> FindPageDefinitions(Assembly pluginAssembly);
}

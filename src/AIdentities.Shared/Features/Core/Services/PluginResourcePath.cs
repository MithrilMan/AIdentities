using AIdentities.Shared.Plugins;

namespace AIdentities.Shared.Features.Core.Services;

public class PluginResourcePath : IPluginResourcePath
{
   public string GetContentPath<TPlugin>(string relativePath) where TPlugin : IPluginEntry
   {
      if (relativePath.StartsWith("/"))
      {
         relativePath = relativePath[1..];
      }

      var nameSpace = typeof(TPlugin).Namespace!;
      var path = $"_content/{nameSpace}/{relativePath}";

      return path;
   }
}

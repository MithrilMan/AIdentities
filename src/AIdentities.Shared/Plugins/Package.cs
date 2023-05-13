using System.Reflection;

namespace AIdentities.Shared.Plugins;

public record Package(
   PluginManifest PluginManifest,
   Assembly Assembly,
   Assembly? Symbols
   )
{
   public bool IsLoaded { get; internal init; }

   public List<(string, string)> Components { get; set; } = new();

   public List<(string, string)> Assets { get; set; } = new();

   public List<(string, string)> RegisteredServices { get; set; } = new();

   /// <summary>
   /// List of PageDefinitions that are defined in the plugin.
   /// </summary>
   public IReadOnlyList<PageDefinition>? Pages { get; init; }
}

namespace AIdentities.Shared.Plugins;

public interface IPluginSignature
{
   /// <summary>
   /// The plugin author.
   /// </summary>
   string Author { get; }

   /// <summary>
   /// The plugin name.
   /// </summary>
   string Name { get; }

   /// <summary>
   /// The plugin version.
   /// </summary>
   string Version { get; }

   /// <summary>
   /// The plugin entry point.
   /// It's the name of the main dll file containing the plugin.
   /// </summary>
   string Entry { get; }

   /// <summary>
   /// Gets the full name of the plugin.
   /// </summary>
   string GetFullName();
}

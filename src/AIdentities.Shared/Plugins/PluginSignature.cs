namespace AIdentities.Shared.Plugins;

public record PluginSignature : IPluginSignature
{
   public string Name { get; set; } = string.Empty;
   public string Version { get; set; } = string.Empty;
   public string Author { get; set; } = string.Empty;

   public string Entry { get; set; } = string.Empty;

   public string GetFullName() => PathUtils.SanitizePath($"{Name}.{Version}");
}

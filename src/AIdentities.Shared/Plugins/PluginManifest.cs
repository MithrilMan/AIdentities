namespace AIdentities.Shared.Plugins;
public record PluginManifest : IPluginSpecification
{
   public PluginSignature Signature { get; set; } = default!;
   public string Description { get; set; } = string.Empty;
   public string Website { get; set; } = string.Empty;
   public string Email { get; set; } = string.Empty;
   public string License { get; set; } = string.Empty;
   public string LicenseUrl { get; set; } = string.Empty;
   public string Icon { get; set; } = string.Empty;
   public IEnumerable<string>? Tags => _tags;
   public IEnumerable<PluginDependency>? Dependencies => _dependencies;

   IPluginSignature IPluginSpecification.Signature => Signature;

   private readonly List<string> _tags = new();
   private readonly List<PluginDependency> _dependencies = new();
}

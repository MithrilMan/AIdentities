namespace AIdentities.Shared.Plugins;

/// <summary>
/// Defines the plugin specification.
/// This maps the content of the aid-manifest.json included in the plugin package.
/// </summary>
public interface IPluginSpecification
{
   IPluginSignature Signature { get; }

   string Description { get; }
   string Website { get; }
   string Email { get; }
   string License { get; }
   string LicenseUrl { get; }
   string Icon { get; }
   IEnumerable<string>? Tags { get; }

   IEnumerable<PluginDependency>? Dependencies { get; }
}

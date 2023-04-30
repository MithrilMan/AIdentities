namespace AIdentities.UI.Features.Settings.Services;

public record PluginStatus(PluginManifest Manifest)
{
   public enum PackageStatus
   {
      Available,
      Activated,
      Invalid,
      Conflicts,
      PendingActivated,
      PendingDisabled,
   }

   public PackageStatus Status { get; set; } = PackageStatus.Available;

   public string? Problems { get; set; }
}

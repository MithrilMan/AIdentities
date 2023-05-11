using AIdentities.Shared.Plugins;

namespace AIdentities.Shared.Features.AIdentities.Services;

/// <summary>
/// This interface has to be implemented by the plugin developer to check how an AIdentity is impacted the feature.
/// The most important method is <see cref="IsAIdentitySafeToBeDeletedAsync(AIdentity, out string)"/> that is used
/// to check if an AIdentity can be deleted safely.
/// In order for a plugin to be able to check the safety of an AIdentity, it has to implement this interface and
/// has to register it into the service collection within <see cref="IPluginEntry.Initialize"/>() method.
/// </summary>
public interface IAIdentitySafetyChecker
{
   /// <summary>
   /// Returns the activity of an AIdentity regarding the plugin.
   /// The plugin developer should implement this method to return the activity of an AIdentity regarding the plugin.
   /// If a plugin doesn't uses explicitly the AIdentity, it can return an empty collection.
   /// </summary>
   /// <param name="aIdentity"></param>
   /// <returns>A collection of <see cref="AIdentityPluginActivity"/> representing the activity of the AIdentity regarding the plugin.</returns>
   ValueTask<AIdentityPluginActivity?> GetAIdentityActivityAsync(AIdentity aIdentity);

   /// <summary>
   /// Check if an AIdentity can be deleted safely.
   /// Plugin developers should implement this method to check if the AIdentity can be deleted safely
   /// e.g. they may have produced some data related to a specific AIdentity that, if the AIdentity is
   /// deleted, may break some features or corrupt some data.
   /// Ensuring that the AIdentity is safe to be deleted is a responsibility of the plugin developer.
   /// The result of this method will be shown to the user in the AIdentity management page when he
   /// tries to delete an AIdentity.
   /// A plugin developer should in any case implement its feature in a way that even if an AIdentity
   /// has been deleted, the plugin will not break.
   /// The result of this method will act anyway as a deterrent for the user.
   /// </summary>
   /// <param name="aIdentity">The AIdentity to check.</param>
   /// If the AIdentity cannot be deleted, the reason why it cannot be deleted.
   /// Even if the AIdentity can be deleted, the plugin developer can provide a reason to the user to not delete it.
   /// </param>
   /// <returns>A tuple containing a boolean indicating if the AIdentity can be deleted and a string 
   /// containing the reason why it cannot (or should not) be deleted.</returns>
   ValueTask<(bool canDelete, string? reasonToNotDelete)> IsAIdentitySafeToBeDeletedAsync(AIdentity aIdentity);
}

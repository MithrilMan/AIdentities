using AIdentities.Shared.Plugins;

namespace AIdentities.Shared.Features.AIdentities.Services;
/// <summary>
/// Return this class to declare that the plugin does not need to register any AIdentity safety checker.
/// For details about what's a AIdentity safety checker, see <see cref="IAIdentitySafetyChecker"/>.
/// </summary>
public sealed class NoAIdentitySafetyChecker : IAIdentitySafetyChecker
{
   static readonly (bool canDelete, string? reasonToNotDelete) _returnValue = (true, null);

   ValueTask<AIdentityPluginActivity?> IAIdentitySafetyChecker.GetAIdentityActivityAsync(AIdentity aIdentity)
      => ValueTask.FromResult<AIdentityPluginActivity?>(null);

   public ValueTask<(bool canDelete, string? reasonToNotDelete)> IsAIdentitySafeToBeDeletedAsync(AIdentity aIdentity)
      => ValueTask.FromResult(_returnValue);
}

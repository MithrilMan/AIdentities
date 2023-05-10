using MudBlazor.Extensions;
using static AIdentities.Shared.Features.AIdentities.Models.AIdentityPluginActivity;

namespace AIdentities.BooruAIdentityImporter;
public class BooruAIdentitySafetyChecker : IAIdentitySafetyChecker
{

   readonly ILogger<BooruAIdentitySafetyChecker> _logger;

   public BooruAIdentitySafetyChecker(ILogger<BooruAIdentitySafetyChecker> logger)
   {
      _logger = logger;
   }

   public async ValueTask<AIdentityPluginActivity?> GetAIdentityActivityAsync(AIdentity aIdentity)
   {
      return new AIdentityPluginActivity() {
         { "Hey", new PluginActivityDetail(0, "hehehe") }
      };
   }

   public async ValueTask<(bool canDelete, string? reasonToNotDelete)> IsAIdentitySafeToBeDeletedAsync(AIdentity aIdentity)
   {

      return (false, "perchè no");
   }
}

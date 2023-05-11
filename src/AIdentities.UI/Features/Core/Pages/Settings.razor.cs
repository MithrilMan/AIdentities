using Microsoft.AspNetCore.Components;

namespace AIdentities.UI.Features.Core.Pages;

public partial class Settings : ComponentBase
{
   /// <summary>
   /// The list of all the Plugin Settings section registered by plugins.
   /// </summary>
   [Inject] IEnumerable<AIdentityFeatureRegistration> PluginSettingRegistration { get; set; } = null!;
}

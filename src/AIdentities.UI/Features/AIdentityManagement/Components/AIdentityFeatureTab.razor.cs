using Microsoft.AspNetCore.Components;

namespace AIdentities.UI.Features.AIdentityManagement.Components;

public partial class AIdentityFeatureTab
//where TFeature : IAIdentityFeature
{
   [Parameter] public AIdentityFeatureRegistration FeatureRegistration { get; set; } = default!;
   [Parameter] public AIdentity AIdentity { get; set; } = default!;

   private readonly Dictionary<string, object?> _parameters = new();

   private DynamicComponent? _featureTab;

   protected override void OnParametersSet()
   {
      base.OnParametersSet();

      _parameters.Clear();
      _parameters["Feature"] = AIdentity?.Features[FeatureRegistration.FeatureType];
   }


   //TODO: move here the save/undo logic and call _featureTab.Instance.SaveAsync() and _featureTab.Instance.UndoChangesAsync()
}

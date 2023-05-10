using AIdentities.Shared.Features.AIdentities.Abstracts;
using Microsoft.AspNetCore.Components;

namespace AIdentities.UI.Features.AIdentityManagement.Components;

public partial class AIdentityFeatureTab
{
   [Inject] public IAIdentityProvider AIdentityProvider { get; set; } = default!;
   [Inject] public INotificationService NotificationService { get; set; } = default!;

   [Parameter] public AIdentityFeatureRegistration FeatureRegistration { get; set; } = default!;
   [Parameter] public AIdentity AIdentity { get; set; } = default!;
   [Parameter] public bool IsEditing { get; set; }
   [Parameter] public EventCallback<bool> IsEditingChanged { get; set; }

   private DynamicComponent? _featureTab;

   protected override void OnInitialized() => base.OnInitialized(); //TODO viene sempre chiamato, quindi il tab lo distrugge

   protected override void OnParametersSet()
   {
      base.OnParametersSet();
      _state.SetCurrentFeature(AIdentity?.Features[FeatureRegistration.FeatureType]);
   }

   public async Task SaveAsync()
   {
      var aidentityFeatureTab = (IAIdentityFeatureTab)_featureTab!.Instance!;
      var result = await aidentityFeatureTab.SaveAsync().ConfigureAwait(false);

      if (result == null)
      {
         NotificationService.ShowWarning("Please fix the errors on the form.");
         return;
      }

      AIdentity.Features[FeatureRegistration.FeatureType] = result;

      AIdentityProvider.Update(AIdentity);

      // update the parameters to apply the needed changes
      _state.SetCurrentFeature(AIdentity?.Features[FeatureRegistration.FeatureType]);

      NotificationService.ShowSuccess("AIdentity updated successfully!");
   }

   public async Task UndoChangesAsync()
   {
      var aidentityFeatureTab = (IAIdentityFeatureTab)_featureTab!.Instance!;
      await aidentityFeatureTab.UndoChangesAsync().ConfigureAwait(false);

      await IsEditingChanged.InvokeAsync(false).ConfigureAwait(false);
   }
}

using AIdentities.Shared.Features.AIdentities.Abstracts;

namespace AIdentities.UI.Features.AIdentityManagement.Components;

public partial class AIdentityFeatureTab
{
   class State
   {
      public object? CurrentFeature { get; private set; }
      public Dictionary<string, object?> Parameters { get; } = new();

      public void SetCurrentFeature(AIdentity aIdentity, object? feature)
      {
         Parameters[nameof(IAIdentityFeatureTab<IAIdentityFeature>.Feature)] = feature;
         Parameters[nameof(IAIdentityFeatureTab.IsChanged)] = feature != CurrentFeature;
         Parameters[nameof(IAIdentityFeatureTab.AIdentity)] = aIdentity;

         CurrentFeature = feature;
      }
   }

   private readonly State _state = new();
}

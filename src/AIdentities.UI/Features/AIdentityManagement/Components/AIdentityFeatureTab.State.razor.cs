namespace AIdentities.UI.Features.AIdentityManagement.Components;

public partial class AIdentityFeatureTab
{
   class State
   {
      public object? CurrentFeature { get; private set; }
      public Dictionary<string, object?> Parameters { get; } = new();

      public void SetCurrentFeature(object? feature)
      {
         Parameters["Feature"] = feature;
         Parameters["IsChanged"] = feature != CurrentFeature;

         CurrentFeature = feature;
      }
   }

   private readonly State _state = new();
}

using AIdentities.Shared.Features.AIdentities.Abstracts;
using Microsoft.AspNetCore.Components;

namespace AIdentities.UI.Features.AIdentityManagement.Components;

public partial class AIdentityFeatureTab<TFeature>
{
   [Parameter] public string Title { get; set; } = typeof(TFeature).Name;
   [Parameter] public TFeature? Feature { get; set; }
   [Parameter] public IAIdentityFeatureTab<TFeature>? FeatureTab { get; set; }

   private readonly Dictionary<string, object?> _parameters = new();

   private DynamicComponent? _featureTab;

   protected override void OnParametersSet()
   {
      base.OnParametersSet();

      _parameters.Clear();
      _parameters["Feature"] = Feature;
   }
}

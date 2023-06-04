using AIdentities.Shared.Features.AIdentities.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace AIdentities.Shared.Features.AIdentities.Components;
partial class AIdentitySelector
{
   [Inject] public IAIdentityProvider AIdentityProvider { get; set; } = default!;

   /// <summary>
   /// Custom CSS classes to apply to the root element of the component.
   /// </summary>
   [Parameter] public string Class { get; set; } = default!;

   /// <summary>
   /// Custom CSS style to apply to the root element of the component.
   /// </summary>
   [Parameter] public string Style { get; set; } = default!;

   /// <summary>
   /// Specifies whether the component should automatically focus on the input element when the page loads.
   /// </summary>
   [Parameter] public bool AutoFocus { get; set; }

   /// <summary>
   /// Sets the specific margin to the inner autocomplete element.
   /// </summary>
   [Parameter] public Margin Margin { get; set; }

   /// <summary>
   /// Sets the variant of the input element.
   /// </summary>
   [Parameter] public Variant Variant { get; set; }

   /// <summary>
   /// The currently selected AIdentity.
   /// </summary>
   [Parameter] public AIdentity? SelectedAIdentity { get; set; }

   /// <summary>
   /// The event that is fired when the selected AIdentity changes.
   /// </summary>
   [Parameter] public EventCallback<AIdentity> SelectedAIdentityChanged { get; set; }

   /// <summary>
   /// The placeholder text to display when no AIdentity is selected.
   /// </summary>
   [Parameter] public string Placeholder { get; set; } = default!;

   /// <summary>
   /// An optional filter to apply to the list of available AIdentities.
   /// By default, all AIdentities are included.
   /// </summary>
   [Parameter] public Func<AIdentity, bool>? Filter { get; set; }

   /// <summary>
   /// a list of AIdentityIds to exclude from the list of available AIdentities.
   /// </summary>
   [Parameter] public IEnumerable<Guid> ExcludedAIdentityIds { get; set; } = default!;

   Task OnSelectedAIdentityChanged() => SelectedAIdentityChanged.InvokeAsync(_state.SelectedAIdentity);
}

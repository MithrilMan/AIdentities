using Microsoft.AspNetCore.Components;

namespace AIdentities.UI.Features.AIdentityManagement.Components;

public partial class AIdentityGallery
{
   [Inject] public IAIdentityProvider AIdentityProvider { get; set; } = default!;

   [Parameter] public AIdentity? AIdentity { get; set; }
   [Parameter] public EventCallback<AIdentity> AIdentityChanged { get; set; }
   [Parameter] public EventCallback<AIdentity> OnEdit { get; set; }
   [Parameter] public EventCallback<AIdentity> OnDelete { get; set; }
   [Parameter] public bool NeedToReload { get; set; }
   [Parameter] public EventCallback<bool> NeedToReloadChanged { get; set; }

   protected override void OnInitialized()
   {
      base.OnInitialized();
      _state.Initialize(Filter);
   }

   protected override async Task OnInitializedAsync()
   {
      await FetchAIdentities().ConfigureAwait(false);
   }

   private async Task FetchAIdentities()
   {
      await _state.AIdentities.LoadItemsAsync(AIdentityProvider.All()).ConfigureAwait(false);
      _state.AvailableAIdentityTags = _state.AIdentities.UnfilteredItems.SelectMany(i => i.Tags).ToHashSet();
      await ApplyFilterAsync().ConfigureAwait(false);
   }

   protected override async Task OnParametersSetAsync()
   {
      await base.OnParametersSetAsync().ConfigureAwait(false);
      if (NeedToReload)
      {
         await FetchAIdentities().ConfigureAwait(false);
         await NeedToReloadChanged.InvokeAsync(false).ConfigureAwait(false);
      }
   }

   public ValueTask<IEnumerable<AIdentity>> Filter(IEnumerable<AIdentity> unfilteredItems)
   {
      if (!string.IsNullOrWhiteSpace(_state.AIdentitySearchText))
      {
         unfilteredItems = unfilteredItems
         .Where(c => c.Name?.Contains(_state.AIdentitySearchText, StringComparison.OrdinalIgnoreCase) ?? false);

         var tags = _state.AIdentityTags.Select(t => t.ToString()).ToList();

         if (_state.AIdentityTags.Any())
         {
            unfilteredItems = unfilteredItems.Where(i => i.Tags.Any(x => tags.Contains(x)));
         }
      }

      unfilteredItems = unfilteredItems.OrderByDescending(c => c.UpdatedAt);

      return ValueTask.FromResult(unfilteredItems);
   }

   private async Task ApplyFilterAsync() => await _state.AIdentities.ApplyFilterAsync().ConfigureAwait(false);

   void ToggleTagSelection(string value)
   {
      if (_state.AIdentityTags.Contains(value))
      {
         ((HashSet<string>)_state.AIdentityTags).Remove(value);
      }
      else
      {
         ((HashSet<string>)_state.AIdentityTags).Add(value);
      }
   }

   Task Delete(AIdentity identity) => OnDelete.InvokeAsync(identity);
   Task Edit(AIdentity identity) => OnEdit.InvokeAsync(identity);
}

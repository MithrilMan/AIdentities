using Microsoft.AspNetCore.Components;

namespace AIdentities.UI.Features.Automation.Components;

public partial class DutyList : ComponentBase
{
   [Parameter] public string? Class { get; set; }

   protected override void OnInitialized()
   {
      base.OnInitialized();
      _state.Initialize(DutyFilter);
   }

   protected override async Task OnInitializedAsync()
   {
      //create 10 instance of Duty class with random values
      var randomDuties = new List<Duty>();
      for (var i = 0; i < 10; i++)
      {
         var duty = new Duty
         {
            Title = $"Duty {i}",
            Description = $"Description {i}"
         };
         randomDuties.Add(duty);
      }
      await _state.Duties.LoadItemsAsync(randomDuties).ConfigureAwait(false);
   }

   public async ValueTask<IEnumerable<Duty>> DutyFilter(IEnumerable<Duty> unfilteredItems)
   {
      if (_state.DutySearchText is null) return unfilteredItems;

      await ValueTask.CompletedTask.ConfigureAwait(false);

      unfilteredItems = unfilteredItems
         .Where(duty =>
            (duty.Title?.Contains(_state.DutySearchText, StringComparison.OrdinalIgnoreCase) ?? false)
            || (duty.Description?.Contains(_state.DutySearchText, StringComparison.OrdinalIgnoreCase) ?? false)
         );

      return unfilteredItems;
   }

   private Task ApplyFilter() => _state.Duties.ApplyFilterAsync().AsTask();
}

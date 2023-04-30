using AIdentities.Shared.Collections;

namespace AIdentities.UI.Features.Automation.Components;

public partial class DutyList
{
   class State
   {
      public string? DutySearchText { get; set; }
      public FilteredObservableCollection<Duty> Duties { get; private set; } = default!;

      public void Initialize(Func<IEnumerable<Duty>, ValueTask<IEnumerable<Duty>>> dutyFilter)
      {
         DutySearchText = null;
         Duties = new(dutyFilter);
      }
   }

   private readonly State _state = new();
}

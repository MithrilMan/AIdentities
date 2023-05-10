using AIdentities.Shared.Collections;

namespace AIdentities.UI.Features.AIdentityManagement.Components;

public partial class AIdentityGallery
{
   class State
   {
      public string? AIdentitySearchText { get; set; }
      public FilteredObservableCollection<AIdentity> AIdentities { get; private set; } = default!;
      public HashSet<string> AvailableAIdentityTags { get; set; } = new HashSet<string>();
      public IEnumerable<string> AIdentityTags { get; set; } = new HashSet<string>();

      public void Initialize(Func<IEnumerable<AIdentity>, ValueTask<IEnumerable<AIdentity>>> filter)
      {
         AIdentitySearchText = null;
         AIdentities = new(filter);
      }
   }

   private readonly State _state = new State();
}

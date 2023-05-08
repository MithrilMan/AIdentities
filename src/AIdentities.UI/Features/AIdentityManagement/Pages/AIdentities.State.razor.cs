using AIdentities.Shared.Validation;

namespace AIdentities.UI.Features.AIdentityManagement.Pages;

public partial class AIdentities
{
   class State
   {
      public AIdentity? CurrentAIDentity { get; set; }

      public bool IsEditing
      {
         get { return CurrentAIDentity != null; }
         set { CurrentAIDentity = null; }
      }

      public int ActivePanelIndex { get; set; }
   }

   private readonly State _state = new State();
}

namespace AIdentities.UI.Features.AIdentityManagement.Pages;

public partial class AIdentities
{
   class State
   {
      public AIdentity? CurrentAIDentity { get; set; }

      /// <summary>
      /// When set to true, the AIdentity gallery will be reloaded.
      /// The gallery will set it back to false after reloading.
      /// </summary>
      public bool NeedToReload { get; set; }

      public bool IsEditing
      {
         get { return CurrentAIDentity != null; }
         set { CurrentAIDentity = null; }
      }

      public int ActivePanelIndex { get; set; }
   }

   private readonly State _state = new State();
}

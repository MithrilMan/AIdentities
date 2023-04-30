namespace AIdentities.UI.Features.Core.Pages;

public partial class MainLayout
{
   class State
   {
      public bool IsDarkMode { get; set; }
      public bool IsDrawerOpen { get; set; }
   }

   readonly State _state = new State();
}

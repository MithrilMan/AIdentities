using Microsoft.AspNetCore.Components;

namespace AIdentities.UI.Features.Automation.Pages;

public partial class Duty : ComponentBase
{
   class State
   {
      public string? Title { get; set; }
      public string? Description { get; set; }
   }

   private readonly State _state = new State();
}

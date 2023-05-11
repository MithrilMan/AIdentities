using Microsoft.AspNetCore.Components;

namespace AIdentities.UI.Features.Core.Pages;

public partial class Settings : ComponentBase
{
   class State
   {
      public string? Message { get; set; }
   }

   private readonly State _state = new State();
}

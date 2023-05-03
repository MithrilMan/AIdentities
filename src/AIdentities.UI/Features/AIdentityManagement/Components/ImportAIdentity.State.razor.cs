using Microsoft.AspNetCore.Components.Forms;

namespace AIdentities.UI.Features.AIdentityManagement.Components;

public partial class ImportAIdentity
{
   class State
   {
      public IBrowserFile? File { get; set; }
      public bool IsDragging { get; set; } = false;
      public IAIdentityImporter? SelectedImporter { get; set; }
      public bool IsEditing => SelectedImporter != null;
   }

   private readonly State _state = new();



   class Validator : BaseValidator<State>
   {
      public Validator()
      {
         RuleFor(x => x.File)
            .NotEmpty();

         RuleFor(x => x.SelectedImporter)
            .NotEmpty();
      }
   }

   private readonly Validator _validator = new();
}

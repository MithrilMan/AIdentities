namespace AIdentities.UI.Features.AIdentityManagement.Components;

public partial class ExportAIdentity
{
   class State
   {
      public string? FileName { get; set; }
      public IAIdentityExporter? SelectedExporter { get; set; }
      public bool IsEditing => SelectedExporter != null;
   }

   private readonly State _state = new();

   class Validator : BaseValidator<State>
   {
      public Validator()
      {
         RuleFor(x => x.FileName)
            .NotEmpty()
            .Must(filename => PathUtils.IsValidFileName(filename!)).WithMessage("Invalid file name");

         RuleFor(x => x.SelectedExporter)
            .NotEmpty();
      }
   }

   private readonly Validator _validator = new();
}

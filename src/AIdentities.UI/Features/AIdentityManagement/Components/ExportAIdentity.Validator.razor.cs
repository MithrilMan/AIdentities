namespace AIdentities.UI.Features.AIdentityManagement.Components;

public partial class ExportAIdentity
{
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

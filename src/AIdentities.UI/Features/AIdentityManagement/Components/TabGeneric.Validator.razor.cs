namespace AIdentities.UI.Features.AIdentityManagement.Components;

public partial class TabGeneric
{
   class Validator : BaseValidator<State>
   {
      public Validator()
      {
         RuleFor(x => x.Name)
            .NotEmpty();

         RuleFor(x => x.Description)
            .NotEmpty();
      }
   }
   readonly Validator _validator = new Validator();
}

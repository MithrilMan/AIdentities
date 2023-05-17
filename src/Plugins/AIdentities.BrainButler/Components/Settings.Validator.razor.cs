using AIdentities.Shared.Validation;
using FluentValidation;

namespace AIdentities.BrainButler.Components;

public partial class Settings
{
   class Validator : BaseValidator<State>
   {
      public Validator()
      {
         RuleFor(x => x.DefaultConversationalConnector)
            .NotEmpty()
            .Must(connector => connector?.Enabled ?? false)
            .WithMessage("The selected connector is not available because it has been disabled.");

         RuleFor(x => x.DefaultCompletionConnector)
            .NotEmpty()
            .Must(connector => connector?.Enabled ?? false)
            .WithMessage("The selected connector is not available because it has been disabled.");
      }
   }

   Validator _validator = new();
}

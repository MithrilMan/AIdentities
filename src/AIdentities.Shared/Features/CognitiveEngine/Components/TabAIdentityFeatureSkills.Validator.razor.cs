using AIdentities.Shared.Validation;
using FluentValidation;

namespace AIdentities.Shared.Features.CognitiveEngine.Components;

public partial class TabAIdentityFeatureSkills
{
   class Validator : BaseValidator<State>
   {
      public Validator()
      {
      }
   }

   readonly Validator _validator = new Validator();
}

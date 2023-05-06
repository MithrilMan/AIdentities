using FluentValidation;

namespace AIdentities.Chat.Components;
public partial class TabAIdentityFeatureChat
{
   class Validator : BaseValidator<State>
   {
      public Validator()
      {
         RuleFor(x => x.FirstMessage).NotEmpty();

         When(s => s.UseFullPrompt, () =>
         {
            RuleFor(x => x.FullPrompt).NotEmpty();
         }).Otherwise(() =>
         {
            RuleFor(x => x.Background).NotEmpty();
         });
      }
   }

   readonly Validator _validator = new Validator();
}

using FluentValidation;

namespace AIdentities.Chat.Components;
public partial class TabChatAIdentityFeature
{
   class Validator : BaseValidator<State>
   {
      public Validator()
      {
         RuleFor(x => x.ExampleMessages)
            .Must(HaveFullExample)
            .WithMessage("All example messages must have a user message and an AIdentity message.");

         When(s => s.UseFullPrompt, () =>
         {
            RuleFor(x => x.FullPrompt).NotEmpty();
         }).Otherwise(() =>
         {
            RuleFor(x => x.Background).NotEmpty();
         });
      }

      private bool HaveFullExample(List<AIdentityUserExchange> list)
      {
         if (list is null) return true;

         foreach (var item in list)
         {
            if (string.IsNullOrEmpty(item.UserMessage) || string.IsNullOrEmpty(item.AIdentityMessage))
            {
               return false;
            }
         }

         return true;
      }
   }

   readonly Validator _validator = new Validator();
}

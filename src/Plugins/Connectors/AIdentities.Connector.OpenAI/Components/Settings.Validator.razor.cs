using AIdentities.Shared.Validation;
using FluentValidation;

namespace AIdentities.Connector.OpenAI.Components;
public partial class Settings
{
   class Validator : BaseValidator<State>
   {
      public Validator()
      {
         RuleFor(x => x.Enabled).NotNull();

         RuleFor(x => x.ChatEndPoint)
            .Must(BeAValidUri)
            .WithMessage("You must provide a valid URI.");

         RuleFor(x => x.CompletionEndPoint)
            .Must(BeAValidUri)
            .WithMessage("You must provide a valid URI.");

         RuleFor(x => x.ApiKey).NotEmpty();

         RuleFor(x => x.DefaultChatModel).NotEmpty();

         RuleFor(x => x.Timeout).NotNull();
      }

      private bool BeAValidUri(string? uri)
      {
         if (uri is null) return false;

         return Uri.TryCreate(uri, UriKind.Absolute, out _);
      }
   }

   readonly Validator _validator = new Validator();
}

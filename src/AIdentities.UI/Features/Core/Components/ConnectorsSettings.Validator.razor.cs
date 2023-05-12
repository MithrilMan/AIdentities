namespace AIdentities.UI.Features.Core.Components;

public partial class ConnectorsSettings
{
   class Validator : BaseValidator<State>
   {
      public Validator()
      {
         RuleFor(x => x.ConnectorEndPoint)
            .NotEmpty()
            .Must(BeValidUri)
            .WithMessage("Please enter a valid URI.");

         //RuleFor(x => x.ConnectorApiKey)
         //   .NotEmpty();
      }

      private bool BeValidUri(string? uriString)
      {
         if (uriString == null) return false;

         // Attempt to parse the string as a URI
         return Uri.TryCreate(uriString, UriKind.Absolute, out Uri? uriResult)
             && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
      }
   }

   private readonly Validator _validator = new();
}

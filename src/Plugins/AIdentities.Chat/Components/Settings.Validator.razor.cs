using FluentValidation;

namespace AIdentities.Chat.Components;
public partial class Settings
{
   class Validator : BaseValidator<State>
   {
      readonly IEnumerable<IConversationalConnector> _connectors;

      public Validator(IEnumerable<IConversationalConnector> connectors, IEnumerable<Shared.Plugins.Connectors.TextToSpeech.ITextToSpeechConnector> textToSpeechConnectors)
      {
         _connectors = connectors.ToList();

         RuleFor(x => x.DefaultConnector)
            .NotEmpty()
            .Must(x => _connectors.Any(y => y.Name == x))
            .WithMessage("The selected connector is not available.")
            .Must(x => _connectors.Any(y => y.Enabled))
            .WithMessage("The selected connector is not available because it has been disabled.");
      }
   }

   Validator _validator = default!;
}

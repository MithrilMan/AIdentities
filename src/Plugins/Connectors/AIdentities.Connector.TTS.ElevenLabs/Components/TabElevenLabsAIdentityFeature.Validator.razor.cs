using FluentValidation;

namespace AIdentities.Connector.TTS.ElevenLabs.Components;

public partial class TabElevenLabsAIdentityFeature
{
   class Validator : BaseValidator<State>
   {
      public Validator()
      {
         RuleFor(x => x.VoiceId)
            .NotEmpty().When(x => x.Customize);
      }
   }

   readonly Validator _validator = new Validator();
}

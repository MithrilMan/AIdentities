using FluentValidation;

namespace AIdentities.Connector.TTS.ElevenLabs.Components;

public partial class TabElevenLabsAIdentityFeature
{
   class Validator : BaseValidator<State>
   {
      public Validator()
      {
      }
   }

   readonly Validator _validator = new Validator();
}

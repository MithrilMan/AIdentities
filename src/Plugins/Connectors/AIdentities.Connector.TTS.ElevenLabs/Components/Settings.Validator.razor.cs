﻿using AIdentities.Shared.Validation;
using FluentValidation;

namespace AIdentities.Connector.TTS.ElevenLabs.Components;
public partial class Settings
{
   class Validator : BaseValidator<State>
   {
      public Validator()
      {
         RuleFor(x => x.Enabled).NotNull();

         RuleFor(x => x.ApiEndpoint)
            .Must(BeAValidUri)
            .WithMessage("You must provide a valid URI.");

         RuleFor(x => x.ApiKey).NotEmpty();

         RuleFor(x => x.DefaultVoiceId).NotEmpty();

         RuleFor(x => x.DefaultTextToSpeechModel).NotEmpty();

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
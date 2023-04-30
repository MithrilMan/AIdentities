using AIdentities.Shared.Validation;
using Entities = AIdentities.Shared.Features.Core;

namespace AIdentities.UI.Features.AIdentity.Pages;

public partial class AIdentities
{
   class State
   {
      public string? Name { get; set; }
      public string? Image { get; set; }
      public string? Background { get; set; }
      public string? Description { get; set; }
      public string? FullPrompt { get; set; }
      public string? Personality { get; set; }
      public string? FirstMessage { get; set; }

      public Entities.AIdentity? CurrentAIDentity { get; set; }
      public bool IsDragging { get; set; } = false;
      public bool IsAdvancedMode { get; set; } = false;

      internal void SetFormFields(Entities.AIdentity? aIdentity)
      {
         Name = aIdentity?.Name;
         Description = aIdentity?.Description;
         Image = aIdentity?.Image;
         Background = aIdentity?.Background;
         FullPrompt = aIdentity?.FullPrompt;
         Personality = aIdentity?.Personality;
         FirstMessage = aIdentity?.FirstMessage;
      }
   }

   private readonly State _state = new State();


   class Validator : BaseValidator<State>
   {
      public Validator()
      {
         RuleFor(x => x.Name)
            .NotEmpty();

         RuleFor(x => x.Description)
            .NotEmpty();

         RuleFor(x => x.FullPrompt)
            .NotEmpty();
      }
   }
   readonly Validator _validator = new Validator();
}

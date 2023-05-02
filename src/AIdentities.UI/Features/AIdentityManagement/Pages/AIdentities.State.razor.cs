using AIdentities.Shared.Validation;

namespace AIdentities.UI.Features.AIdentityManagement.Pages;

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

      public AIdentity? CurrentAIDentity { get; set; }
      public bool IsDragging { get; set; } = false;
      public bool UseFullPrompt { get; set; } = false;

      internal void SetFormFields(AIdentity? aIdentity)
      {
         Name = aIdentity?.Name;
         Description = aIdentity?.Description;
         Image = aIdentity?.Image;
         Background = aIdentity?.Background;
         FullPrompt = aIdentity?.FullPrompt;
         Personality = aIdentity?.Personality;
         FirstMessage = aIdentity?.FirstMessage;
         UseFullPrompt = aIdentity?.UseFullPrompt ?? false;
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

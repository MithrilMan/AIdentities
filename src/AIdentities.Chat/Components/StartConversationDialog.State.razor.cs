using FluentValidation;
using AIdentities.Shared.Features.Core;

namespace AIdentities.Chat.Components;

public partial class StartConversationDialog
{
   class State
   {
      public AIdentity? SelectedAIdentity { get; set; }
   }

   private readonly State _state = new();



   class Validator : BaseValidator<State>
   {
      public Validator()
      {
         RuleFor(x => x.SelectedAIdentity)
             .NotEmpty();
      }
   }

   private readonly Validator _validator = new();
}

namespace AIdentities.Shared.Features.AIdentities.Components;

public partial class AIdentitySelector
{
   class State
   {
      public AIdentity? SelectedAIdentity { get; set; }
   }

   private readonly State _state = new();
}

namespace AIdentities.Shared.Features.CognitiveEngine.Models;

public record AIdentitiesConstraint
{
   public List<AIdentity>? AllowedAIdentities { get; set; }
}

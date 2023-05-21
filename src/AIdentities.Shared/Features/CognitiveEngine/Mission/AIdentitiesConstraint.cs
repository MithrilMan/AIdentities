namespace AIdentities.Shared.Features.CognitiveEngine.Mission;

public record AIdentitiesConstraint
{
   public List<AIdentity>? AllowedAIdentities { get; set; }
}

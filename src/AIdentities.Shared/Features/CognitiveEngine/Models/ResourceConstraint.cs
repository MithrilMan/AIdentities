namespace AIdentities.Shared.Features.CognitiveEngine.Models;

public record ResourceConstraints
{
   public TimeSpan MaxAllowedTimeToExecute { get; set; }
}

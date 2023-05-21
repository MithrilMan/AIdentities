namespace AIdentities.Shared.Features.CognitiveEngine.Mission;

public record ResourceConstraints
{
   /// <summary>
   /// The maximum allowed time to execute the mission.
   /// By default it's set to TimeSpan.MaxValue (no time limit).
   /// </summary>
   public TimeSpan MaxAllowedTimeToExecute { get; set; } = TimeSpan.MaxValue;
}

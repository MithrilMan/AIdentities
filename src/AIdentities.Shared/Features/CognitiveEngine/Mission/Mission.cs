namespace AIdentities.Shared.Features.CognitiveEngine.Mission;

/// <summary>
/// What the user wants to achieve.
/// A mission can have a set of constraints that affect the way the cognitive engine will work.
/// </summary>
public record Mission
{
   /// <summary>
   /// The goal of the mission.
   /// </summary>
   public string Goal { get; set; } = "";

   /// <summary>
   /// The mission context.
   /// </summary>
   public MissionContext Context { get; set; } = new();
}

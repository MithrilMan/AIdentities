namespace AIdentities.Shared.Features.CognitiveEngine.Mission;

/// <summary>
/// What the user wants to achieve.
/// A mission can have a set of constraints that affect the way the cognitive engine will work.
/// </summary>
public interface IMission
{
   /// <summary>
   /// The goal of the mission.
   /// </summary>
   public string Goal { get; }

   /// <summary>
   /// The mission context.
   /// </summary>
   MissionContext Context { get; }
}

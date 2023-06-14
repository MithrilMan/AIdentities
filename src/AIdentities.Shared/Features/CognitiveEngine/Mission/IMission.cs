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

   /// <summary>
   /// Returns true if the mission is running.
   /// </summary>
   bool IsRunning { get; }

   /// <summary>
   /// The cognitive engine that's responsible for running the mission.
   /// </summary>
   ICognitiveEngine? MissionRunner { get; }

   /// <summary>
   /// This token is valid while the mission is running.
   /// When the mission is stopped, this token is cancelled.
   /// This token can be used to stop the execution of async operations that are part of the mission.
   /// </summary>
   CancellationToken MissionRunningCancellationToken { get; }

   /// <summary>
   /// Start the mission.
   /// </summary>
   /// <param name="cognitiveEngine">The cognitive engine that's responsible for running the mission.</param>
   /// <param name="missionCancellationToken">The cancellation token that can be used to stop the mission.</param>
   void Start(ICognitiveEngine cognitiveEngine, CancellationToken missionCancellationToken);

   void Stop();
}

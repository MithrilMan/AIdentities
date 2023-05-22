namespace AIdentities.Shared.Features.CognitiveEngine.Mission;

/// <summary>
/// What the user wants to achieve.
/// A mission can have a set of constraints that affect the way the cognitive engine will work.
/// </summary>
public class Mission<TMissionContext> : IMission
   where TMissionContext : MissionContext, new()
{
   /// <inheritdoc/>
   public string Goal { get; set; } = "";

   /// <summary>
   /// The specialized mission context.
   /// </summary>
   public TMissionContext Context { get; set; } = new();

   /// <inheritdoc/>
   public CancellationToken MissionRunningCancellationToken { get; internal set; }

   /// <inheritdoc/>
   MissionContext IMission.Context { get => Context; }
}

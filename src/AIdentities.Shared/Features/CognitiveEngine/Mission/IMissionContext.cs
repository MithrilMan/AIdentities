namespace AIdentities.Shared.Features.CognitiveEngine.Mission;

public interface IMissionContext
{
   /// <summary>
   /// This token is valid while the mission is running.
   /// When the mission is stopped, this token is cancelled.
   /// This token can be used to stop the execution of async operations that are part of the mission.
   /// </summary>
   public CancellationToken MissionRunningCancellationToken { get;  }

   /// <summary>
   /// The AIdentities constraints of the mission.
   /// </summary>
   AIdentitiesConstraint AIdentitiesConstraints { get; }
   /// <summary>
   /// The resource constraints of the mission.
   /// </summary>
   ResourceConstraints ResourceConstraints { get; }
   /// <summary>
   /// The skill constraints of the mission.
   /// Which skills are available in the mission and (optional) which AIdentity is preferred for each skill.
   /// </summary>
   List<SkillConstraint> SkillConstraints { get; }
   /// <summary>
   /// Hold any state information that can be used by any skill and AIdentity in the mission.
   /// A sort of collective meaningfull memory.
   /// </summary>
   Dictionary<string, object> State { get; }
}

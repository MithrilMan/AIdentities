namespace AIdentities.Shared.Features.CognitiveEngine.Mission;

/// <summary>
/// This class is used to pass information between skills execution within a mission context.
/// At any point any AIdentity skill can add or remove information from the context.
/// </summary>
public record MissionContext
{
   /// <summary>
   /// The resource constraints of the mission.
   /// </summary>
   public ResourceConstraints ResourceConstraints { get; init; } = new();

   /// <summary>
   /// The skill constraints of the mission.
   /// Which skills are available in the mission and (optional) which AIdentity is preferred for each skill.
   /// </summary>
   public List<SkillConstraint> SkillConstraints { get; init; } = new();

   /// <summary>
   /// The AIdentities constraints of the mission.
   /// </summary>
   public List<AIdentitiesConstraint> AIdentitiesConstraints { get; init; } = new();

   public Dictionary<string, object> StateObjects { get; init; } = new();
}

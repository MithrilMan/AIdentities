namespace AIdentities.Shared.Features.CognitiveEngine.Models;

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
   /// The resource constraints of the mission.
   /// </summary>
   public ResourceConstraints ResourceConstraints { get; set; } = new();

   /// <summary>
   /// The skill constraints of the mission.
   /// Which skills are available in the mission and (optional) which AIdentity is preferred for each skill.
   /// </summary>
   public List<SkillConstraint> SkillConstraints { get; set; } = new();

   /// <summary>
   /// The AIdentities constraints of the mission.
   /// </summary>
   public List<AIdentitiesConstraint> AIdentitiesConstraints { get; set; } = new();

   /// <summary>
   /// The mission context.
   /// </summary>
   public MissionContext Context { get; set; } = new();
}

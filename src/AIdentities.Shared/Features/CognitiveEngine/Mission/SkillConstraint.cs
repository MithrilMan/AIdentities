namespace AIdentities.Shared.Features.CognitiveEngine.Mission;

public record SkillConstraint
{
   /// <summary>
   /// The available skill.
   /// </summary>
   public SkillDefinition Skill { get; set; }

   /// <summary>
   /// The preferred AIdentity for the skill.
   /// If null, the AIdentity running the mission will chose the AIdentity for the skill.
   /// </summary>
   public AIdentity? PreferredAIdentity { get; set; }

   public SkillConstraint(SkillDefinition skill, AIdentity? preferredAIdentity)
   {
      Skill = skill;
      PreferredAIdentity = preferredAIdentity;
   }
}

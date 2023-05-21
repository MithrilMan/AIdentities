namespace AIdentities.Shared.Features.CognitiveEngine.Mission;

public record Skill
{
   public Type SkillType { get; set; }
   public string Name { get; set; }
   public string Description { get; set; }
}

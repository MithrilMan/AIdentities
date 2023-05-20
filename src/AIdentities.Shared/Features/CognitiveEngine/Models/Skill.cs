namespace AIdentities.Shared.Features.CognitiveEngine.Models;

public record Skill
{
   public Type SkillType { get; set; }
   public string Name { get; set; }
   public string Description { get; set; }
}

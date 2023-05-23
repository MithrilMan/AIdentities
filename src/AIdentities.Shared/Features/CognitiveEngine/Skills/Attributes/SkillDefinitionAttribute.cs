namespace AIdentities.Shared.Features.CognitiveEngine.Skills.Attributes;

/// <summary>
/// Attribute used to define a skill.
/// To define a skill the user need to use multiple attributes:
/// - <see cref="SkillDefinitionAttribute"/> to define the skill itself.
/// - <see cref="SkillInputDefinitionAttribute"/> to define the input of the skill.
/// - <see cref="SkillOutputDefinitionAttribute"/> to define the output of the skill.
/// - <see cref="SkillExampleAttribute"/> to define an example of how to use the skill.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class SkillDefinitionAttribute : Attribute
{
   /// <summary>
   /// Name of the skill.
   /// </summary>
   public string? Name { get; init; }

   /// <summary>
   /// The description of the skill.
   /// This is very important because it's used by the LLM to find the best skill to accomplish a task.
   /// </summary>
   public string? Description { get; init; }

   /// <summary>
   /// A list of tags that can be used to find the skill.
   /// </summary>
   public List<string> Tags { get; init; } = new();
}

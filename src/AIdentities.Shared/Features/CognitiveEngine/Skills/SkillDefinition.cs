namespace AIdentities.Shared.Features.CognitiveEngine.Skills;

/// <summary>
/// Contains the definition of a skill.
/// It's generated during startup by scaffolding available
/// skills and access their skill definition attributes.
/// It's used to create proper prompts for the LLM to
/// be able to select a tool or format properly the inputs and
/// outputs of the skill.
/// </summary>
public record SkillDefinition(string Name, string Description)
{
   /// <summary>
   /// Name of the skill.
   /// </summary>
   public string Name { get; init; } = Name;

   /// <summary>
   /// The description of the skill.
   /// This is very important because it's used by the LLM to find the best skill to accomplish a task.
   /// </summary>
   public string Description { get; init; } = Description;

   /// <summary>
   /// A list of examples of how to use the skill.
   /// </summary>
   public List<SkillExample> Examples { get; init; } = new List<SkillExample>();

   /// <summary>
   /// A list of tags that can be used to find the skill.
   /// </summary>
   public List<string> Tags { get; init; } = new List<string>();

   /// <summary>
   /// The list of inputs of the skill.
   /// </summary>
   public List<SkillInputDefinition> Inputs { get; init; } = new List<SkillInputDefinition>();

   /// <summary>
   /// The list of outputs of the skill.
   /// </summary>
   public List<SkillOutputDefinition> Outputs { get; init; } = new List<SkillOutputDefinition>();
}

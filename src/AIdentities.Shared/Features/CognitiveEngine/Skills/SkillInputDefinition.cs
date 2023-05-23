namespace AIdentities.Shared.Features.CognitiveEngine.Skills;

/// <summary>
/// Defines an input of a skill.
/// </summary>
public record SkillInputDefinition(string Name, SkillVariableType Type, bool IsRequired, string Description)
{
   /// <summary>
   /// The name of the variable.
   /// </summary>
   public string Name { get; init; } = Name;

   /// <summary>
   /// The description of the variable.
   /// This is very important for the LLM to understand the meaning of the variable.
   /// </summary>
   public string Description { get; init; } = Description;

   /// <summary>
   /// The type of the argument.
   /// </summary>
   public SkillVariableType Type { get; init; } = Type;

   /// <summary>
   /// Specifies whether the argument is required.
   /// </summary>
   public bool IsRequired { get; init; } = IsRequired;
}

namespace AIdentities.Shared.Features.CognitiveEngine.Skills.Attributes;

/// <summary>
/// Attribute used to define an input of a skill.
/// All the fields are important and required.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class SkillInputDefinitionAttribute : Attribute
{
   /// <summary>
   /// The name of the variable.
   /// The LLM will use this name to populate the variables needed for the skill.
   /// </summary>
   public string? Name { get; init; }

   /// <summary>
   /// The description of the variable.
   /// This is very important for the LLM to understand the meaning of the variable.
   /// </summary>
   public string? Description { get; init; }

   /// <summary>
   /// The type of the argument.
   /// </summary>
   public SkillVariableType Type { get; init; } = SkillVariableType.Unknown;

   /// <summary>
   /// Specifies whether the argument is required.
   /// </summary>
   public bool IsRequired { get; init; } = false;
}

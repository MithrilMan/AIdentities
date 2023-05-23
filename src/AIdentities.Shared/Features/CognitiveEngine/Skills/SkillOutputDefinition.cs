namespace AIdentities.Shared.Features.CognitiveEngine.Skills;

/// <summary>
/// Defines an output of a skill.
/// </summary>
public record SkillOutputDefinition(string Name, string Description, SkillVariableType Type)
{
   /// <summary>
   /// The name of the variable.
   /// The LLM will use this name to read the variable after the action has been executed.
   /// </summary>
   public string Name { get; init; } = Name;

   /// <summary>
   /// The description of the variable.
   /// This is very important for the LLM to understand the meaning of this output variable.
   /// </summary>
   public string Description { get; init; } = Description;

   /// <summary>
   /// The type of the argument.
   /// This is important to format the argument in the right way.
   /// </summary>
   public SkillVariableType Type { get; init; } = Type;

   /// <summary>
   /// Specify if this output is also generated as a final thought (either streamed or non-streamed).
   /// A final thought is a thought that is generated at the end of the skill execution 
   /// and could be displayed to the user.
   /// </summary>
   public bool AsFinalThought { get; init; } = false;
}

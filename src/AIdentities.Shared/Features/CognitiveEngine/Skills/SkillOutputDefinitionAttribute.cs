namespace AIdentities.Shared.Features.CognitiveEngine.Skills;


[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class SkillOutputDefinitionAttribute : Attribute
{
   /// <summary>
   /// The name of the variable.
   /// </summary>
   public string Name { get; init; } = default!;

   /// <summary>
   /// The description of the variable.
   /// This is very important for the LLM to understand the meaning of the variable.
   /// </summary>
   public string Description { get; init; } = default!;

   /// <summary>
   /// The type of the argument.
   /// </summary>
   public SkillVariableType Type { get; init; } = default!;
}

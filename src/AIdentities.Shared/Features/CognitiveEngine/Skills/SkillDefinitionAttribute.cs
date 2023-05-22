namespace AIdentities.Shared.Features.CognitiveEngine.Skills;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]

public class SkillDefinitionAttribute : Attribute
{
   public Type SkillType { get; init; } = default!;

   /// <summary>
   /// Name of the skill.
   /// </summary>
   public string Name { get; init; } = default!;

   /// <summary>
   /// Ver
   /// </summary>
   public string ActivationContext { get; init; } = default!;

   public List<string> Tags { get; init; } = new();
}

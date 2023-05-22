namespace AIdentities.Shared.Features.CognitiveEngine.Skills;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class SkillExampleAttribute : Attribute
{
   public string Example { get; init; } = default!;
}

namespace AIdentities.Shared.Features.CognitiveEngine.Thoughts;

/// <summary>
/// When a skill requires arguments that aren't provided, it will return this thought.
/// The caller should then provide the missing arguments.
/// </summary>
public record MissingArgumentsThought : Thought
{
   public IEnumerable<SkillArgumentDefinition> MissingArguments { get; }

   public MissingArgumentsThought(Guid? SkillActionId, AIdentity AIdentity, IEnumerable<SkillArgumentDefinition> missingArguments)
      : base(SkillActionId, AIdentity.Id, "Not enough data to process the request.")
   {
      MissingArguments = missingArguments;
   }
}

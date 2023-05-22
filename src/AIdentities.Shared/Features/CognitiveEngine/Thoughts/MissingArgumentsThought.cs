namespace AIdentities.Shared.Features.CognitiveEngine.Thoughts;

/// <summary>
/// When a skill requires arguments that aren't provided, it will return this thought.
/// The caller should then provide the missing arguments.
/// This is a special case of <see cref="IntrospectiveThought"/>.
/// </summary>
public record MissingArgumentsThought : IntrospectiveThought
{
   public IEnumerable<SkillArgumentDefinition> MissingArguments { get; }

   public MissingArgumentsThought(Guid? SkillActionId, AIdentity AIdentity, IEnumerable<SkillArgumentDefinition> missingArguments)
      : base(SkillActionId, AIdentity, "Not enough data to process the request.")
   {
      MissingArguments = missingArguments;
   }
}

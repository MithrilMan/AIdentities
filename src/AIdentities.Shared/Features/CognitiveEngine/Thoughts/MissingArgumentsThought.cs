namespace AIdentities.Shared.Features.CognitiveEngine.Thoughts;

/// <summary>
/// When a skill requires arguments that aren't provided, it will return this thought.
/// The caller should then provide the missing arguments.
/// This is a special case of <see cref="IntrospectiveThought"/>.
/// </summary>
public record MissingArgumentsThought : IntrospectiveThought
{
   public IEnumerable<string> MissingArguments { get; }

   public MissingArgumentsThought(string? skillName, AIdentity aIdentity, IEnumerable<string> missingArguments)
      : base(skillName, aIdentity, "Not enough data to process the request.")
   {
      MissingArguments = missingArguments;
   }
}

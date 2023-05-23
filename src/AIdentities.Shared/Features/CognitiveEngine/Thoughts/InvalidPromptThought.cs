namespace AIdentities.Shared.Features.CognitiveEngine.Thoughts;

/// <summary>
/// When a skill doesn't have the required arguments or the format are wrong, it will return this thought.
/// Some skills need for example a valid Json object as a prompt, but the prompt provided something else.
/// The description of the thought should contains information about the error and how to fix it, to let
/// the LLM try to fix it or to let the user know what is wrong.
/// </summary>
public record InvalidArgumentsThought : IntrospectiveThought
{
   public InvalidArgumentsThought(string? skillName, AIdentity aIdentity, string howToFix)
      : base(skillName, aIdentity, howToFix)
   {
   }
}

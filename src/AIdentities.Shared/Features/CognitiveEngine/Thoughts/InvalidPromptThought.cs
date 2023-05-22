namespace AIdentities.Shared.Features.CognitiveEngine.Thoughts;

/// <summary>
/// When a skill receives an invalid prompt, it will return this thought.
/// Some skills need for example a valid Json object as a prompt, but the prompt provided something else.
/// </summary>
public record InvalidPromptThought : IntrospectiveThought
{
   public InvalidPromptThought(Guid? SkillActionId, AIdentity AIdentity)
      : base(SkillActionId, AIdentity, "I couldn't extract a proper JSON object from the prompt to extract arguments I need.")
   {
   }
}

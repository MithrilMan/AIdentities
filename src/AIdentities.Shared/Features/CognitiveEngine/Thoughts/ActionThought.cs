namespace AIdentities.Shared.Features.CognitiveEngine.Thoughts;

/// <summary>
/// An action thought is a thought that is created when the cognitive engine has enough data to
/// process the request and have therefore to do something in order to proceed.
/// It has to be considered quite like a log of what the cognitive engine is doing.
/// </summary>
public record ActionThought : Thought
{
   public ActionThought(Guid? SkillActionId, AIdentity AIdentity, string Content)
      : base(SkillActionId, AIdentity.Id, Content)
   {
   }
}

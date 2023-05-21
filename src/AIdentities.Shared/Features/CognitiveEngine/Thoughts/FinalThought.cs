namespace AIdentities.Shared.Features.CognitiveEngine.Thoughts;

/// <summary>
/// A final thought is something that returns a semantic result that should be returned to the user.
/// </summary>
public record FinalThought : Thought
{
   public FinalThought(Guid? SkillActionId, Guid AIdentityId, string Content) : base(SkillActionId, AIdentityId, Content)
   {
      IsFinal = true;
   }
}

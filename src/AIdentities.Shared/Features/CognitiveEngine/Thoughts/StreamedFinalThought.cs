namespace AIdentities.Shared.Features.CognitiveEngine.Thoughts;

/// <summary>
/// A final thought is something that returns a semantic result that should be returned to the user.
/// </summary>
public record StreamedFinalThought : StreamedThought
{
   public StreamedFinalThought(Guid? SkillActionId, Guid AIdentityId, string Content, bool IsLastStreamPiece) : base(SkillActionId, AIdentityId, Content, IsLastStreamPiece)
   {
      IsFinal = true;
   }
}

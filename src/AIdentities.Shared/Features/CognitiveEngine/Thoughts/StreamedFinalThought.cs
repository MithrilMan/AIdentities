namespace AIdentities.Shared.Features.CognitiveEngine.Thoughts;

/// <summary>
/// A streamed final thought is something that returns a semantic result that should be returned to the user.
/// See <see cref="StreamedThought"/> for more information.
/// </summary>
public record StreamedFinalThought : StreamedThought
{
   public StreamedFinalThought(Guid? SkillActionId, Guid AIdentityId, string Content, bool IsStreamComplete = false)
      : base(SkillActionId, AIdentityId, Content, IsStreamComplete)
   { }
}

namespace AIdentities.Shared.Features.CognitiveEngine.Thoughts;

/// <summary>
/// A streamed introspective thought is an <see cref="IntrospectiveThought"/> that gets generated like
/// a <see cref="StreamedThought"/>.
/// See respective documentations for more information.
/// </summary>
public record StreamedIntrospectiveThought : StreamedThought
{
   public StreamedIntrospectiveThought(Guid? SkillActionId, AIdentity AIdentity, string Content, bool IsStreamComplete = false)
      : base(SkillActionId, AIdentity.Id, Content, IsStreamComplete) { }
}

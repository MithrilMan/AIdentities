namespace AIdentities.Shared.Features.CognitiveEngine.Thoughts;

/// <summary>
/// A streamed introspective thought is an <see cref="IntrospectiveThought"/> that gets generated like
/// a <see cref="StreamedThought"/>.
/// See respective documentations for more information.
/// </summary>
public record StreamedIntrospectiveThought : StreamedThought, IIntrospectiveThought
{
   public StreamedIntrospectiveThought(string? skillName, AIdentity aIdentity, string content, bool isStreamComplete = false)
      : base(skillName, aIdentity, content, isStreamComplete) { }
}

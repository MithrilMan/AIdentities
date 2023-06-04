namespace AIdentities.Shared.Features.CognitiveEngine.Thoughts;

/// <summary>
/// A streamed final thought is something that returns a semantic result that should be returned to the user.
/// See <see cref="StreamedThought"/> for more information.
/// </summary>
public record StreamedFinalThought : StreamedThought, IFinalThought
{
   public StreamedFinalThought(string? skillName, AIdentity aIdentity, string content, bool isStreamComplete = false)
      : base(skillName, aIdentity, content, isStreamComplete)
   { }
}

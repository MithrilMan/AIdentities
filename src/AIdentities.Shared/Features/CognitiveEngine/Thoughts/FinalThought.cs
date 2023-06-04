namespace AIdentities.Shared.Features.CognitiveEngine.Thoughts;

/// <summary>
/// A final thought is something that returns a semantic result that should be returned to the user.
/// </summary>
public record FinalThought : Thought, IFinalThought
{
   public FinalThought(string? skillName, AIdentity aIdentity, string content)
      : base(skillName, aIdentity.Id, content) { }
}

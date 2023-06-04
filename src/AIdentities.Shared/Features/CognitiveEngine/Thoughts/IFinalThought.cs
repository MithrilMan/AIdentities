namespace AIdentities.Shared.Features.CognitiveEngine.Thoughts;

/// <summary>
/// A final thought is something that returns a semantic result that should be returned to the user.
/// To not have to check both streamed and non-streamed results, we use this interface to mark the final thoughts.
/// </summary>
public interface IFinalThought : IThought { }

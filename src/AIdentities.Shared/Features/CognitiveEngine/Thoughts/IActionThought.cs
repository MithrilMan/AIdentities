namespace AIdentities.Shared.Features.CognitiveEngine.Thoughts;

/// <summary>
/// An action thought is a thought that is created when the cognitive engine has something to
/// annotate about what it is doing.
/// It's usually not meaningful for the user, but it can be useful for debugging.
/// It could be considered quite like a log of what the cognitive engine is doing.
/// </summary>
public interface IActionThought : IThought { }

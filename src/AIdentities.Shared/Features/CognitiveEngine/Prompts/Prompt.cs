namespace AIdentities.Shared.Features.CognitiveEngine.Prompts;

/// <summary>
/// A basic, abstract prompt.
/// </summary>
/// <param name="Text">The text of the prompt.</param>
public abstract record Prompt(string Text) { }

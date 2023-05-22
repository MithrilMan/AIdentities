namespace AIdentities.Shared.Features.CognitiveEngine.Prompts;

/// <summary>
/// This prompt is the result of an AIdentity generated prompt for someone else.
/// </summary>
/// <param name="AIdentityId">The Id of the AIdentity that generated the prompt.</param>
/// <param name="Text">The text of the prompt.</param>
public record AIdentityPrompt(Guid AIdentityId, string Text) : Prompt(Text) { }

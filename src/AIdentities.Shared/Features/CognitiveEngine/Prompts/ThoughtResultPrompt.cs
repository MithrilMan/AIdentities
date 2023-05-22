namespace AIdentities.Shared.Features.CognitiveEngine.Prompts;

/// <summary>
/// This prompt is the result of a self-thought execution.
/// </summary>
/// <param name="AIdentityId">The Id of the AIdentity that generated the prompt.</param>
/// <param name="Text">The text of the prompt.</param>
public record ThoughtResultPrompt(Guid AIdentityId, string Text) : AIdentityPrompt(AIdentityId, Text) { }

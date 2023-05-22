namespace AIdentities.Shared.Features.CognitiveEngine.Prompts;

/// <summary>
/// This prompt is an instruction prompt that the receiver should use
/// and analyze to understand the request.
/// </summary>
/// <param name="Text">The instruction text.</param>
public record InstructionPrompt(string Text) : Prompt(Text) { }

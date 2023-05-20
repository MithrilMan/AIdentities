using AIdentities.Shared.Features.CognitiveEngine.Models;

namespace AIdentities.Shared.Features.CognitiveEngine;
public interface ICognitiveEngine
{
   AIdentity AIdentity { get; }
   CognitiveContext Context { get; }

   IAsyncEnumerable<Thought> HandlePrompt(Prompt prompt);
}
using AIdentities.Shared.Features.CognitiveEngine.Mission;
using AIdentities.Shared.Features.CognitiveEngine.Thoughts;

namespace AIdentities.Shared.Features.CognitiveEngine;

/// <summary>
/// Represents the cognitive engine of an AIdentity (it's brain basically).
/// There can be different implementations of this interface that differs on how
/// each one handles the prompts and coordinate a common goal with others.
/// </summary>
public interface ICognitiveEngine
{
   /// <summary>
   /// The AIdentity that owns this cognitive engine.
   /// Or in other words, the AIdentity that this cognitive engine is the brain of.
   /// </summary>
   AIdentity AIdentity { get; }

   /// <summary>
   /// The context of the cognitive engine.
   /// Think of it like the working memory of the cognitive engine that can be shared among skills
   /// executed by this AIdentity in a specific context.
   /// </summary>
   CognitiveContext Context { get; }

   /// <summary>
   /// Handle a prompt, returning a stream of thoguhts.
   /// </summary>
   /// <param name="prompt">The prompt to handle.</param>
   /// <param name="missionContext">
   /// An optional mission context.
   /// If mission context is provided, the cognitive engine will try to use the mission context to decide how to proceed and
   /// when / if delegate actions to other AIdentities.
   /// </param>
   /// <param name="cancellationToken">The cancellation token.</param>
   /// <returns>A stream of thoughts generated during the prompt handling.</returns>
   IAsyncEnumerable<Thought> HandlePromptAsync(Prompt prompt, MissionContext? missionContext, CancellationToken cancellationToken);
}

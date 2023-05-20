using AIdentities.Shared.Features.CognitiveEngine.Models;
using AIdentities.Shared.Plugins.Connectors.Conversational;

namespace AIdentities.Shared.Features.CognitiveEngine;

/// <summary>
/// The abstract cognitive engine class.
/// There can be different implementations of this class that differs on how
/// each one handles the prompts.
/// </summary>
public abstract class CognitiveEngine : ICognitiveEngine
{
   public AIdentity AIdentity { get; }
   public CognitiveContext Context { get; } = new();

   protected DefaultConversationalMessage PersonalityInstruction { get; private set; } = default!;

   public CognitiveEngine(AIdentity aIdentity)
   {
      AIdentity = aIdentity;

      EnsureAIdentityIsValid();
      SetupAIdentityPersonality();
   }

   /// <summary>
   /// Ensure that the AIdentity is valid and meet the minimum requirements.
   /// </summary>
   /// <exception cref="ArgumentNullException"></exception>
   protected virtual void EnsureAIdentityIsValid()
   {
      if (AIdentity is null) throw new ArgumentNullException(nameof(AIdentity));
      if (AIdentity.Name is null) throw new ArgumentNullException(nameof(AIdentity.Name));
      if (AIdentity.Personality is null) throw new ArgumentNullException(nameof(AIdentity.Personality));
   }

   /// <summary>
   /// Setup the personality of the AIdentity.
   /// </summary>
   protected virtual void SetupAIdentityPersonality()
   {
      PersonalityInstruction = new DefaultConversationalMessage(
                 Role: DefaultConversationalRole.System,
                 Content: AIdentity.Personality!,
                 Name: AIdentity.Name);
   }

   /// <summary>
   /// Handle a prompt, returning a stream of thoguhts.
   /// 
   /// </summary>
   /// <param name="prompt"></param>
   /// <returns></returns>
   /// <exception cref="NotImplementedException"></exception>
   public abstract IAsyncEnumerable<Thought> HandlePrompt(Prompt prompt);
}

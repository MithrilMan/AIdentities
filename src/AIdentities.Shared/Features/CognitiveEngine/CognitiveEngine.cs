using AIdentities.Shared.Features.CognitiveEngine.Mission;
using AIdentities.Shared.Features.CognitiveEngine.Thoughts;
using AIdentities.Shared.Plugins.Connectors.Completion;
using AIdentities.Shared.Plugins.Connectors.Conversational;

namespace AIdentities.Shared.Features.CognitiveEngine;

/// <summary>
/// The abstract cognitive engine class.
/// There can be different implementations of this class that differs on how
/// each one handles the prompts.
/// </summary>
public abstract class CognitiveEngine<TCognitiveContext> : ICognitiveEngine
   where TCognitiveContext : CognitiveContext
{
   protected readonly ILogger _logger;
   protected readonly IConversationalConnector _defaultConversationalConnector;
   protected readonly ICompletionConnector _defaultCompletionConnector;
   protected readonly ISkillActionsManager _skillActionsManager;

   public AIdentity AIdentity { get; }

   public TCognitiveContext Context { get; }

   CognitiveContext ICognitiveEngine.Context => Context;

   protected DefaultConversationalMessage PersonalityInstruction { get; private set; } = default!;

   public CognitiveEngine(ILogger logger,
                          AIdentity aIdentity,
                          IConversationalConnector defaultConversationalConnector,
                          ICompletionConnector defaultCompletionConnector,
                          ISkillActionsManager skillActionsManager)
   {
      _logger = logger;
      AIdentity = aIdentity;
      _defaultConversationalConnector = defaultConversationalConnector;
      _defaultCompletionConnector = defaultCompletionConnector;
      _skillActionsManager = skillActionsManager;

      EnsureAIdentityIsValid();

      Context = CreateCognitiveContext();

      SetupAIdentityPersonality();
   }

   public abstract TCognitiveContext CreateCognitiveContext();

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

   public abstract IAsyncEnumerable<Thought> HandlePromptAsync(Prompt prompt, MissionContext? missionContext, CancellationToken cancellationToken);
}

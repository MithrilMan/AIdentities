using AIdentities.Shared.Features.CognitiveEngine.Mission;
using AIdentities.Shared.Features.CognitiveEngine.Prompts;
using AIdentities.Shared.Features.CognitiveEngine.Thoughts;
using AIdentities.Shared.Plugins.Connectors.Completion;
using AIdentities.Shared.Plugins.Connectors.Conversational;

namespace AIdentities.Shared.Features.CognitiveEngine.Engines.Reflexive;

/// <summary>
/// This cognitive engine is the default ReflexiveCognitiveEngine that tries to reflect upon itself to perform any task it can.
/// When a prompt is given to it, it tries to find the best skill to accomplish it.
/// If it can't find a skill, it calls the <see cref="HandleNoCommandDetected(Prompt, IMissionContext?, CancellationToken)"/> method.
/// If a skill is found, it executes it and waits for the result of such skill.
/// If the detected skill is unknown (sometimes LLM can hallucinate commands instructions we give it), it calls the
/// <see cref="HandleUnknownCommandDetected(Prompt, string, IMissionContext?, CancellationToken)"/> method, this may give the chance
/// to reissue the prompt to re-evaluate it, or to perform a different action.
/// If the skill has trouble executing, it tries to find a conversation piece to reply to the user.
/// </summary>
public class DefaultReflexiveCognitiveEngine : ReflexiveCognitiveEngine<CognitiveContext>
{
   public DefaultReflexiveCognitiveEngine(ILogger<DefaultReflexiveCognitiveEngine> logger,
                                AIdentity aIdentity,
                                IConversationalConnector defaultConversationalConnector,
                                ICompletionConnector defaultCompletionConnector,
                                ISkillManager skillManager)
     : base(logger, aIdentity, defaultConversationalConnector, defaultCompletionConnector, skillManager) { }

   public override CognitiveContext CreateCognitiveContext() => new CognitiveContext(AIdentity);

   protected override IAsyncEnumerable<Thought> HandleNoCommandDetected(
      Prompt prompt,
      IMissionContext? missionContext,
      CancellationToken cancellationToken)
   {
      return base.HandleNoCommandDetected(prompt, missionContext, cancellationToken);
   }
}

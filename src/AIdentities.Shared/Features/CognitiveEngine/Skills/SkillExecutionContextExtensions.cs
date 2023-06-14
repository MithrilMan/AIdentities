using AIdentities.Shared.Plugins.Connectors.Completion;
using AIdentities.Shared.Plugins.Connectors.Conversational;

namespace AIdentities.Shared.Features.CognitiveEngine.Skills;
public static class SkillExecutionContextExtensions
{
   /// <summary>
   /// Gets the default conversational connector.
   /// If a skill doesn't depend explicitly on a connector, it should use the one defined in the cognitive engine.
   /// </summary>
   /// <param name="context">The skill execution context.</param>
   /// <returns>The default conversational connector.</returns>
   /// <exception cref="InvalidOperationException">No conversational connector is enabled</exception>
   public static IConversationalConnector GetDefaultConversationalConnector(this SkillExecutionContext context)
   {
      return context.CognitiveContext.CognitiveEngine.GetDefaultConnector<IConversationalConnector>()
         ?? throw new InvalidOperationException("No conversational connector is enabled");
   }

   /// <summary>
   /// Gets the default completion connector.
   /// If a skill doesn't depend explicitly on a connector, it should use the one defined in the cognitive engine.
   /// </summary>
   /// <param name="context">The skill execution context.</param>
   /// <returns>The default completion connector.</returns>
   /// <exception cref="InvalidOperationException">No completion connector is enabled</exception>
   public static ICompletionConnector GetDefaultCompletionConnector(this SkillExecutionContext context)
   {
      return context.CognitiveContext.CognitiveEngine.GetDefaultConnector<ICompletionConnector>()
         ?? throw new InvalidOperationException("No completion connector is enabled");
   }
}

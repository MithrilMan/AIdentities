using AIdentities.Shared.Plugins.Connectors;
using AIdentities.Shared.Plugins.Connectors.Completion;
using AIdentities.Shared.Plugins.Connectors.Conversational;
using Microsoft.Extensions.DependencyInjection;

namespace AIdentities.Shared.Features.CognitiveEngine.Engines.Reflexive;

public class DefaultReflexiveCognitiveEngineFactory : ICognitiveEngineFactory
{
   readonly IServiceProvider _serviceProvider;
   readonly IConnectorsManager<IConversationalConnector> _conversationalConnectors;
   readonly IConnectorsManager<ICompletionConnector> _completionConnectors;

   public Type CognitiveEngineType { get; } = typeof(DefaultReflexiveCognitiveEngine);

   public DefaultReflexiveCognitiveEngineFactory(
      IServiceProvider serviceProvider,
      IConnectorsManager<IConversationalConnector> conversationalConnectors,
      IConnectorsManager<ICompletionConnector> completionConnectors
      )
   {
      _serviceProvider = serviceProvider;
      _conversationalConnectors = conversationalConnectors;
      _completionConnectors = completionConnectors;
   }

   public ICognitiveEngine CreateCognitiveEngine(AIdentity aIdentity)
   {
      var cognitiveEngine = new DefaultReflexiveCognitiveEngine(
         logger: _serviceProvider.GetRequiredService<ILogger<DefaultReflexiveCognitiveEngine>>(),
         aIdentity: aIdentity,
         defaultConversationalConnector: _conversationalConnectors.GetFirstEnabled() ?? throw new InvalidOperationException("No conversational connector is enabled."),
         defaultCompletionConnector: _completionConnectors.GetFirstEnabled() ?? throw new InvalidOperationException("No conversational connector is enabled."),
         skillManager: _serviceProvider.GetRequiredService<ISkillManager>()
         );

      return cognitiveEngine;
   }
}

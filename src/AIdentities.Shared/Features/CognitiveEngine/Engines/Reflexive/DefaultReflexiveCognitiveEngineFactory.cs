using AIdentities.Shared.Plugins.Connectors;
using Microsoft.Extensions.DependencyInjection;

namespace AIdentities.Shared.Features.CognitiveEngine.Engines.Reflexive;

public class DefaultReflexiveCognitiveEngineFactory : ICognitiveEngineFactory
{
   readonly IServiceProvider _serviceProvider;
   readonly IDefaultConnectors _defaultConnectors;

   public Type CognitiveEngineType { get; } = typeof(DefaultReflexiveCognitiveEngine);

   public DefaultReflexiveCognitiveEngineFactory(
      IServiceProvider serviceProvider,
      IDefaultConnectors defaultConnectors)
   {
      _serviceProvider = serviceProvider;
      _defaultConnectors = defaultConnectors;
   }

   public ICognitiveEngine CreateCognitiveEngine(AIdentity aIdentity)
   {
      var cognitiveEngine = new DefaultReflexiveCognitiveEngine(
         logger: _serviceProvider.GetRequiredService<ILogger<DefaultReflexiveCognitiveEngine>>(),
         aIdentity: aIdentity,
         defaultConversationalConnector: _defaultConnectors.DefaultConversationalConnector,
         defaultCompletionConnector: _defaultConnectors.DefaultCompletionConnector,
         skillManager: _serviceProvider.GetRequiredService<ISkillManager>()
         );

      return cognitiveEngine;
   }
}

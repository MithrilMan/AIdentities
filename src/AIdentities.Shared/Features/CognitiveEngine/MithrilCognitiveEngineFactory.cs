using AIdentities.Shared.Features.CognitiveEngine.Engines.Mithril;
using AIdentities.Shared.Plugins.Connectors;
using Microsoft.Extensions.DependencyInjection;

namespace AIdentities.Shared.Features.CognitiveEngine;

public class MithrilCognitiveEngineFactory : ICognitiveEngineFactory
{
   readonly IServiceProvider _serviceProvider;
   readonly IDefaultConnectors _defaultConnectors;

   public Type CognitiveEngineType { get; } = typeof(MithrilCognitiveEngine);

   public MithrilCognitiveEngineFactory(
      IServiceProvider serviceProvider,
      IDefaultConnectors defaultConnectors)
   {
      _serviceProvider = serviceProvider;
      _defaultConnectors = defaultConnectors;
   }

   public ICognitiveEngine CreateCognitiveEngine(AIdentity aIdentity)
   {
      var cognitiveEngine = new MithrilCognitiveEngine(
         logger: _serviceProvider.GetRequiredService<ILogger<MithrilCognitiveEngine>>(),
         aIdentity: aIdentity,
         defaultConversationalConnector: _defaultConnectors.DefaultConversationalConnector,
         defaultCompletionConnector: _defaultConnectors.DefaultCompletionConnector,
         skillActionsManager: _serviceProvider.GetRequiredService<ISkillManager>()
         );

      return cognitiveEngine;
   }
}

using AIdentities.Shared.Features.CognitiveEngine.Engines.Mithril;
using AIdentities.Shared.Plugins.Connectors;
using Microsoft.Extensions.DependencyInjection;

namespace AIdentities.Shared.Features.CognitiveEngine;

public class CognitiveEngineProvider : ICognitiveEngineProvider
{
   readonly ILogger<CognitiveEngineProvider> _logger;
   readonly IServiceProvider _serviceProvider;
   readonly IDefaultConnectors _defaultConnectors;

   public CognitiveEngineProvider(ILogger<CognitiveEngineProvider> logger, IServiceProvider serviceProvider, IDefaultConnectors defaultConnectors)
   {
      _logger = logger;
      _serviceProvider = serviceProvider;
      _defaultConnectors = defaultConnectors;
   }

   public ICognitiveEngine CreateCognitiveEngine<TCognitiveEngine>(AIdentity aIdentity)
      where TCognitiveEngine : ICognitiveEngine
   {
      var cognitiveEngine = new MithrilCognitiveEngine(
         logger: _serviceProvider.GetRequiredService<ILogger<MithrilCognitiveEngine>>(),
         aIdentity: aIdentity,
         defaultConversationalConnector: _defaultConnectors.DefaultConversationalConnector,
         defaultCompletionConnector: _defaultConnectors.DefaultCompletionConnector,
         skillActionsManager: _serviceProvider.GetRequiredService<ISkillActionsManager>()
         );

      return cognitiveEngine;
   }
}

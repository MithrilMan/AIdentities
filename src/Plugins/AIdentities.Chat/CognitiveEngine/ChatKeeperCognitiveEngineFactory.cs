using Microsoft.Extensions.DependencyInjection;

namespace AIdentities.Chat.CognitiveEngine;

public class ChatKeeperCognitiveEngineFactory : ICognitiveEngineFactory
{
   readonly IServiceProvider _serviceProvider;
   readonly IDefaultConnectors _defaultConnectors;

   public Type CognitiveEngineType { get; } = typeof(ChatKeeperCognitiveEngine);

   public ChatKeeperCognitiveEngineFactory(
      IServiceProvider serviceProvider,
      IDefaultConnectors defaultConnectors)
   {
      _serviceProvider = serviceProvider;
      _defaultConnectors = defaultConnectors;
   }

   public ICognitiveEngine CreateCognitiveEngine(AIdentity aIdentity)
   {
      var cognitiveEngine = new ChatKeeperCognitiveEngine(
         logger: _serviceProvider.GetRequiredService<ILogger<ChatKeeperCognitiveEngine>>(),
         aIdentity: aIdentity,
         defaultConversationalConnector: _defaultConnectors.DefaultConversationalConnector,
         defaultCompletionConnector: _defaultConnectors.DefaultCompletionConnector,
         skillManager: _serviceProvider.GetRequiredService<ISkillManager>()
         );

      return cognitiveEngine;
   }
}

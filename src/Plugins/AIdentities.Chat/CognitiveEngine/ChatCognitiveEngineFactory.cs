using AIdentities.Shared.Features.CognitiveEngine.Engines.Conversational;
using Microsoft.Extensions.DependencyInjection;

namespace AIdentities.Chat.CognitiveEngine;

public class ChatCognitiveEngineFactory : ICognitiveEngineFactory
{
   readonly IServiceProvider _serviceProvider;
   readonly IDefaultConnectors _defaultConnectors;

   public Type CognitiveEngineType { get; } = typeof(ChatCognitiveEngine);

   public ChatCognitiveEngineFactory(
      IServiceProvider serviceProvider,
      IDefaultConnectors defaultConnectors)
   {
      _serviceProvider = serviceProvider;
      _defaultConnectors = defaultConnectors;
   }

   public ICognitiveEngine CreateCognitiveEngine(AIdentity aIdentity)
   {
      var cognitiveEngine = new ChatCognitiveEngine(
         logger: _serviceProvider.GetRequiredService<ILogger<ChatCognitiveEngine>>(),
         aIdentity: aIdentity,
         defaultConversationalConnector: _defaultConnectors.DefaultConversationalConnector,
         defaultCompletionConnector: _defaultConnectors.DefaultCompletionConnector,
         skillManager: _serviceProvider.GetRequiredService<ISkillManager>()
         );

      return cognitiveEngine;
   }
}

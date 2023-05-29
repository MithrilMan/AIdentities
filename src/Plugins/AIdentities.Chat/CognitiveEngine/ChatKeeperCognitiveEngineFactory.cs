using AIdentities.Shared.Plugins.Connectors.Completion;
using Microsoft.Extensions.DependencyInjection;

namespace AIdentities.Chat.CognitiveEngine;

public class ChatKeeperCognitiveEngineFactory : ICognitiveEngineFactory
{
   readonly IServiceProvider _serviceProvider;
   readonly IConnectorsManager<IConversationalConnector> _conversationalConnectors;
   readonly IConnectorsManager<ICompletionConnector> _completionConnectors;

   public Type CognitiveEngineType { get; } = typeof(ChatKeeperCognitiveEngine);

   public ChatKeeperCognitiveEngineFactory(
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
      var cognitiveEngine = new ChatKeeperCognitiveEngine(
         logger: _serviceProvider.GetRequiredService<ILogger<ChatKeeperCognitiveEngine>>(),
         aIdentity: aIdentity,
         defaultConversationalConnector: _conversationalConnectors.GetFirstEnabled() ?? throw new InvalidOperationException("No conversational connector is enabled."),
         defaultCompletionConnector: _completionConnectors.GetFirstEnabled() ?? throw new InvalidOperationException("No conversational connector is enabled."),
         skillManager: _serviceProvider.GetRequiredService<ISkillManager>()
         );

      return cognitiveEngine;
   }
}

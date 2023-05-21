using AIdentities.Shared.Plugins.Connectors.Completion;
using AIdentities.Shared.Plugins.Connectors.Conversational;

namespace AIdentities.Shared.Plugins.Connectors;
public class DefaultConnectors : IDefaultConnectors
{
   readonly IEnumerable<IConversationalConnector> _conversationalConnectors;
   readonly IEnumerable<ICompletionConnector> _completionConnectors;

   public DefaultConnectors(IEnumerable<IConversationalConnector> conversationalConnectors, IEnumerable<ICompletionConnector> completionConnectors)
   {
      _conversationalConnectors = conversationalConnectors;
      _completionConnectors = completionConnectors;
   }

   public IConversationalConnector DefaultConversationalConnector
      => _conversationalConnectors.FirstOrDefault(c => c.Enabled)
      ?? throw new InvalidOperationException("No default conversational connector found.");

   public ICompletionConnector DefaultCompletionConnector
      => _completionConnectors.FirstOrDefault(c => c.Enabled)
      ?? throw new InvalidOperationException("No default completion connector found.");
}

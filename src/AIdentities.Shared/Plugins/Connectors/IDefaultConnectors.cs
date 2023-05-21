using AIdentities.Shared.Plugins.Connectors.Completion;
using AIdentities.Shared.Plugins.Connectors.Conversational;

namespace AIdentities.Shared.Plugins.Connectors;

/// <summary>
/// Returns the default connectors for the application.
/// </summary>
public interface IDefaultConnectors
{
   ICompletionConnector DefaultCompletionConnector { get; }
   IConversationalConnector DefaultConversationalConnector { get; }
}

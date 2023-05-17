namespace AIdentities.BrainButler.Services;

public interface IConnectorsManager
{
   /// <summary>
   /// Returns the configured connector that can be used to ask for completion.
   /// It is possible that this returns null if the connector is not configured or if the configuration is invalid.
   /// If a connector configured to be used is not enabled, this will return null.
   /// </summary>
   /// <returns>The configured connector that can be used to ask for completion.</returns>
   ICompletionConnector? GetCompletionConnector();

   /// <summary>
   /// Returns the configured connector that can be used to ask for conversational completion (e.g. OpenAI chat completion).
   /// It is possible that this returns null if the connector is not configured or if the configuration is invalid.
   /// If a connector configured to be used is not enabled, this will return null.
   /// </summary>
   /// <returns>The configured connector that can be used to ask for conversational completion.</returns>
   IConversationalConnector? GetConversationalConnector();

   /// <summary>
   /// Returns all the conversational connectors that are available.
   /// </summary>
   /// <returns>All the conversational connectors that are available.</returns>
   IEnumerable<IConversationalConnector> GetAllConversationalConnectors();

   /// <summary>
   /// Returns all the completion connectors that are available.
   /// </summary>
   /// <returns>All the completion connectors that are available.</returns>
   IEnumerable<ICompletionConnector> GetAllCompletionConnectors();
}

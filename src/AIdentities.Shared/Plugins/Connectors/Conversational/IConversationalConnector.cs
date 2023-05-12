namespace AIdentities.Shared.Plugins.Connectors.Conversational;

/// <summary>
/// To be used for endpoints that support conversations.
/// E.g. OpenAI Chat, Oobabooga, etc.
/// </summary>
public interface IConversationalConnector : IEndpointConnector
{
   /// <summary>
   /// Perform a request to the conversational endpoint.
   /// </summary>
   /// <param name="request">The request to perform.</param>
   /// <returns>The response from the chat API.</returns>
   Task<IConversationalResponse?> RequestChatCompletionAsync(IConversationalRequest request);

   /// <summary>
   /// Perform a request to the conversational endpoint.
   /// </summary>
   /// <param name="request">The request to perform.</param>
   /// <param name="cancellationToken">The cancellation token to stop the stream generation.</param>
   /// <returns>The responses from the stream up to completion.</returns>
   IAsyncEnumerable<IConversationalStreamedResponse> RequestChatCompletionAsStreamAsync(IConversationalRequest request, CancellationToken cancellationToken);

   /// <summary>
   /// Returns the settings for this connector.
   /// </summary>
   /// <returns></returns>
   IConversationalConnectorSettings GetSettings();

   /// <summary>
   /// Sets the settings for this connector.
   /// </summary>
   /// <param name="settings">The settings to set.</param>
   Task SetSettings(IConversationalConnectorSettings settings);
}

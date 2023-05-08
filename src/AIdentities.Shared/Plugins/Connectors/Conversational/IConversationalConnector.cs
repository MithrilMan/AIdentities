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
   /// <returns>The response from the endpoint.</returns>
   Task<IConversionalResponse> Request(IConversationalRequest request);
}

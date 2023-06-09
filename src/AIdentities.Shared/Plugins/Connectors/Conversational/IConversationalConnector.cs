﻿namespace AIdentities.Shared.Plugins.Connectors.Conversational;

/// <summary>
/// To be used for endpoints that support conversations.
/// E.g. OpenAI Chat APIs, TextGeneration, etc.
/// </summary>
public interface IConversationalConnector : IConnector
{
   /// <summary>
   /// Perform a request to the conversational endpoint.
   /// </summary>
   /// <param name="request">The request to perform.</param>
   /// <returns>The response from the chat API.</returns>
   Task<IConversationalResponse?> RequestChatCompletionAsync(IConversationalRequest request, CancellationToken cancellationToken);

   /// <summary>
   /// Perform a request to the conversational endpoint.
   /// </summary>
   /// <param name="request">The request to perform.</param>
   /// <param name="cancellationToken">The cancellation token to stop the stream generation.</param>
   /// <returns>The responses from the stream up to completion.</returns>
   IAsyncEnumerable<IConversationalStreamedResponse> RequestChatCompletionAsStreamAsync(IConversationalRequest request, CancellationToken cancellationToken);
}

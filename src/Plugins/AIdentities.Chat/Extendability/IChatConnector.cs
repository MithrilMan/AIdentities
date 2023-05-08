namespace AIdentities.Chat.Extendability;

public interface IChatConnector : IEndpointConnector
{
   /// <summary>
   /// Requests a chat completion from the chat API.
   /// </summary>
   /// <param name="request">The request to send to the chat API.</param>
   /// <returns>The response from the chat API.</returns>
   Task<ChatApiResponse?> RequestChatCompletionAsync(ChatApiRequest request);

   /// <summary>
   /// Requests a chat completion from the chat API as a stream.
   /// </summary>
   /// <param name="request">The request to send to the chat API.</param>
   /// <param name="cancellationToken">The cancellation token.</param>
   /// <returns>The responses from the stream up to completion.</returns>
   IAsyncEnumerable<ChatApiResponse> RequestChatCompletionAsStreamAsync(ChatApiRequest request, CancellationToken cancellationToken);
}

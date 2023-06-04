namespace AIdentities.Shared.Plugins.Connectors.Completion;

/// <summary>
/// To be used for endpoints that support completion.
/// 
/// </summary>
public interface ICompletionConnector : IConnector
{
   /// <summary>
   /// Perform a request to the conversational endpoint.
   /// </summary>
   /// <param name="request">The request to perform.</param>
   /// <returns>The response from the chat API.</returns>
   Task<ICompletionResponse?> RequestCompletionAsync(ICompletionRequest request, CancellationToken cancellationToken);

   /// <summary>
   /// Perform a request to the conversational endpoint.
   /// </summary>
   /// <param name="request">The request to perform.</param>
   /// <param name="cancellationToken">The cancellation token to stop the stream generation.</param>
   /// <returns>The responses from the stream up to completion.</returns>
   IAsyncEnumerable<ICompletionStreamedResponse> RequestCompletionAsStreamAsync(ICompletionRequest request, CancellationToken cancellationToken);
}

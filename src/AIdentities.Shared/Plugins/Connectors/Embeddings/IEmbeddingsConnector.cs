namespace AIdentities.Shared.Plugins.Connectors.Embeddings;

/// <summary>
/// A connector for embeddings.
/// Embeddings are a way to represent text as a vector of numbers in order to perform operations on them like
/// calculating the similarity between two texts.
/// </summary>
public interface IEmbeddingsConnector
{
   /// <summary>
   /// Perform a request to generate embeddings for the given input.
   /// </summary>
   /// <param name="request">The embeddings request to perform.</param>
   /// <returns>The response from the chat API.</returns>
   Task<EmbeddingResponse> RequestEmbeddings(EmbeddingRequest request, CancellationToken cancellationToken);
}

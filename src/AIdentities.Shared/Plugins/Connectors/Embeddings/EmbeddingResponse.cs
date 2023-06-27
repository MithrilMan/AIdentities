namespace AIdentities.Shared.Plugins.Connectors.Embeddings;

/// <summary>
/// Represents a response from the embeddings connector.
/// </summary>
public record EmbeddingResponse(List<float> Values)
{
   /// <summary>
   /// The generated embeddings.
   /// </summary>
   public List<float> Values { get; } = Values;
}

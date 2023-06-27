namespace AIdentities.Shared.Plugins.Connectors.Embeddings;

/// <summary>
/// A basic embedding request.
/// Properties aren't guaranteed to be supported by all connectors.
/// </summary>
public record EmbeddingRequest(string ModelId, string Input)
{
   /// <summary>
   /// The ID of the model to use.
   /// </summary>
   public string? ModelId { get; } = ModelId;

   /// <summary>
   /// The text to generate embeddings for.
   /// </summary>
   public string Input { get; } = Input;

   /// <summary>
   /// The optional user ID to associate with this request.
   /// </summary>
   public string? UserId { get; init; }   
}

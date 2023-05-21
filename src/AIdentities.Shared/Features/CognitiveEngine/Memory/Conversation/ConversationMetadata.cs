using Microsoft.AspNetCore.Http.Features;

namespace AIdentities.Shared.Features.CognitiveEngine.Memory.Conversation;

/// <summary>
/// Contains the metadata of a conversation.
/// </summary>
public record ConversationMetadata
{
   public int Version { get; set; } = 1;
   public Guid ConversationId { get; set; }

   /// <summary>
   /// The date and time the conversation was created.
   /// </summary>
   public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

   /// <summary>
   /// Contains a list of human Ids that are part of this conversation.
   /// It's technically an hashset so we don't have to bother with duplicates.
   /// </summary>
   public ICollection<Guid> Humans { get; init; } = new HashSet<Guid>();

   /// <summary>
   /// Contains a list of AIdentity Ids that are part of this conversation.
   /// It's technically an hashset so we don't have to bother with duplicates.
   /// </summary>
   public ICollection<Guid> AIdentityIds { get; init; } = new HashSet<Guid>();

   /// <summary>
   /// The title of the conversation.
   /// </summary>
   public string Title { get; set; } = string.Empty;

   /// <summary>
   /// The date and time the conversation was last updated.
   /// </summary>
   public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

   /// <summary>
   /// The number of messages in the conversation.
   /// </summary>
   public int MessageCount { get; set; } = 0;

   /// <summary>
   /// The features of the Conversation.
   /// This is an extension point Plugin developers can use this to manage custom features
   /// to conversations or access other plugins' features.
   /// </summary>
   public FeatureCollection? Features { get; set; }
}

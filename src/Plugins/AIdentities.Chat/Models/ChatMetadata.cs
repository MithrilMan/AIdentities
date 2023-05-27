namespace AIdentities.Chat.Models;

public record ChatMetadata
{
   public int Version { get; set; } = 1;
   public Guid ConversationId { get; set; }
   public Guid? UserId { get; set; }

   /// <summary>
   /// Contains a list of AIdentity Ids that are part of this conversation.
   /// It's technically an hashset so we don't have to bother with duplicates.
   /// </summary>
   public ICollection<Guid> AIdentityIds { get; set; } = new HashSet<Guid>();

   public string Title { get; set; } = string.Empty;
   public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
   public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
   public int MessageCount { get; set; } = 0;
}

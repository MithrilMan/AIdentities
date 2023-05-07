namespace AIdentities.Chat.Models;

public record ConversationMetadata
{
   public Guid ConversationId { get; set; }
   public Guid? UserId { get; set; }
   public Guid? AIdentityId { get; set; }
   public string Title { get; set; } = string.Empty;
   public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
   public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
   public int MessageCount { get; set; } = 0;
}

using AIdentities.Shared.Common;

namespace AIdentities.Chat.Models;

public record Conversation : Entity
{
   public ConversationMetadata Metadata { get; set; } = default!;
   public List<ChatMessage>? Messages { get; set; }
}

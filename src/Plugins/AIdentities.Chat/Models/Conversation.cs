namespace AIdentities.Chat.Models;

public record Conversation
{
   /// <summary>
   /// The id of the conversation
   /// </summary>
   public Guid Id { get; set; } = Guid.NewGuid();

   public ConversationMetadata Metadata { get; set; } = default!;
   public List<ChatMessage>? Messages { get; set; }
}

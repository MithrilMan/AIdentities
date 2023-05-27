namespace AIdentities.Chat.Models;

public record ChatBlock
{
   /// <summary>
   /// The id of the conversation
   /// </summary>
   public Guid Id { get; set; } = Guid.NewGuid();

   public ChatMetadata Metadata { get; set; } = default!;
   public List<ChatMessage>? Messages { get; set; }
}

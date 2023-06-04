namespace AIdentities.BrainButler.Models;

public record ConversationPiece
{
   /// <summary>
   /// The id of this conversation piece.
   /// </summary>
   public Guid Id { get; set; } = Guid.NewGuid();

   /// <summary>
   /// The date and time the message was created.
   /// </summary>
   public DateTimeOffset CreationDate { get; set; } = DateTimeOffset.Now;

   /// <summary>
   /// The message text.
   /// </summary>
   public string? Message { get; set; }
}

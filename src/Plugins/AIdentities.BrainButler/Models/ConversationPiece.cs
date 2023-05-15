using AIdentities.Shared.Common;

namespace AIdentities.BrainButler.Models;
public record ConversationPiece : Entity
{
   /// <summary>
   /// The date and time the message was created.
   /// </summary>
   public DateTimeOffset CreationDate { get; set; } = DateTimeOffset.Now;

   /// <summary>
   /// The message text.
   /// </summary>
   public string? Message { get; set; }
}

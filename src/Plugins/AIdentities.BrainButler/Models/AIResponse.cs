namespace AIdentities.BrainButler.Models;

/// <summary>
/// Defines an AI response to an <see cref="UserRequest"/>.
/// </summary>
public record AIResponse : ConversationPiece
{
   /// <summary>
   /// The user can mark a response as useful or not useful.
   /// </summary>
   public bool? IsUseful { get; set; }

   /// <summary>
   /// The identified action corresponding to the user request.
   /// It can be null if no action was identified.
   /// </summary>
   public Guid? IdentifiedCommandId { get; set; }
}

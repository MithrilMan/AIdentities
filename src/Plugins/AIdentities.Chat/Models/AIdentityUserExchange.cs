namespace AIdentities.Chat.Models;

/// <summary>
/// An single exchange of messages between the AIdentity and the player.
/// </summary>
public record AIdentityUserExchange
{
   /// <summary>
   /// The example of a message sent by the user.
   /// </summary>
   public string UserMessage { get; set; } 

   /// <summary>
   /// The example of a message sent by the AIdentity.
   /// </summary>
   public string AIdentityMessage { get; set; } 
}

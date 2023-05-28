namespace AIdentities.Chat.Models;

public enum TextToSpeechMode
{
   /// <summary>
   /// Every message sent by an AIdentity will be read out loud.
   /// </summary>
   Automatic,

   /// <summary>
   /// The user will be able to hear the message by clicking on a button at a message level.
   /// </summary>
   OnDemand
}

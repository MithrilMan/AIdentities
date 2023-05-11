namespace AIdentities.Chat.Models;

/// <summary>
/// A key-value collection representing the activity of an AIdentity regarding a specific plugin.
/// To ease the use of this class, explicit properties can be defined to access the values.
/// </summary>
class ChatAIdentityPluginActivity : AIdentityPluginActivity
{
   const string ACTIVITY_CONVERSATIONS = "Conversations";

   /// <summary>
   /// The number of conversations the AIdentity has had.
   /// </summary>
   public PluginActivityDetail Conversations
   {
      get => this[ACTIVITY_CONVERSATIONS];
      set => this[ACTIVITY_CONVERSATIONS] = value;
   }
}

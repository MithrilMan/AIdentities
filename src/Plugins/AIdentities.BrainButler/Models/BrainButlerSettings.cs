namespace AIdentities.BrainButler.Models;

public class BrainButlerSettings : IPluginSettings
{
   public const bool DEFAULT_ENABLED = true;

   /// <summary>
   /// The name of the default completion connector to use.
   /// The completion connector is used during the interpretation of the user
   /// input and usually to execute commands.
   /// </summary>
   public string? DefaultCompletionConnector { get; set; }

   /// <summary>
   /// The name of the default conversational connector to use.
   /// The conversational connector is used to reformulate the output of executed commands
   /// and to make the conversation more natural with the user.
   /// Some command may also use the conversational connector internally.
   /// </summary>
   public string? DefaultConversationalConnector { get; set; }
}

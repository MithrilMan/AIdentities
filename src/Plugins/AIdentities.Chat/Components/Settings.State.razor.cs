namespace AIdentities.Chat.Components;
public partial class Settings
{
   public class State : BaseState
   {
      public string? DefaultConnector { get; set; } = default!;
      public Dictionary<string, IConversationalConnector> AvailableConnectors { get; internal set; } = new();

      public override void SetFormFields(ChatSettings pluginSettings)
      {
         pluginSettings ??= new();
         DefaultConnector = pluginSettings.DefaultConnector;
      }
   }
}

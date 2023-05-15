namespace AIdentities.Connector.TextGeneration.Components;

public partial class Settings
{
   public class State : BaseState
   {
      public bool? Enabled { get; set; }
      public string? ChatEndPoint { get; set; } = default!;
      public string? StreamedChatEndPoint { get; set; } = default!;
      public string? DefaultModel { get; set; }
      public int? Timeout { get; set; }

      public TextGenerationParameters Parameters { get; set; } = new();

      public IEnumerable<string> StoppingStrings { get; set; } = new List<string>();

      public override void SetFormFields(TextGenerationSettings pluginSettings)
      {
         pluginSettings ??= new();
         Enabled = pluginSettings.Enabled;
         DefaultModel = pluginSettings.DefaultModel;
         ChatEndPoint = pluginSettings.ChatEndPoint?.ToString();
         StreamedChatEndPoint = pluginSettings.StreamedChatEndPoint?.ToString();
         Timeout = pluginSettings.Timeout;
      }
   }
}

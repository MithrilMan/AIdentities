using AIdentities.Connector.OpenAI.Models;

namespace AIdentities.Connector.OpenAI.Components;
public partial class Settings
{
   public class State : BaseState
   {
      public bool? Enabled { get; set; }
      public string? ChatEndPoint { get; set; } = default!;
      public string? DefaultChatModel { get; set; }
      public string? CompletionEndPoint { get; set; } = default!;
      public string? DefaultCompletionModel { get; set; }
      public string? ApiKey { get; set; } = default!;
      public int? Timeout { get; set; }

      public override void SetFormFields(OpenAISettings pluginSettings)
      {
         pluginSettings ??= new();
         Enabled = pluginSettings.Enabled;
         ChatEndPoint = pluginSettings.ChatEndPoint?.ToString();
         DefaultChatModel = pluginSettings.DefaultChatModel;
         CompletionEndPoint = pluginSettings.CompletionEndPoint?.ToString();
         DefaultCompletionModel = pluginSettings.DefaultCompletionModel;
         ApiKey = pluginSettings.ApiKey;
         Timeout = pluginSettings.Timeout;
      }
   }
}

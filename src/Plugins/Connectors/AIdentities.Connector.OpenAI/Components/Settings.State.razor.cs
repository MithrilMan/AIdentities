using AIdentities.Connector.OpenAI.Models;

namespace AIdentities.Connector.OpenAI.Components;
public partial class Settings
{
   public class State : BaseState
   {
      public bool? Enabled { get; set; }
      public string? ChatEndPoint{ get; set; } = default!;
      public string? ApiKey { get; set; } = default!;
      public string? DefaultModel { get; set; }
      public int? Timeout { get; set; }

      public override void SetFormFields(OpenAISettings pluginSettings)
      {
         pluginSettings ??= new();
         Enabled = pluginSettings.Enabled;
         DefaultModel = pluginSettings.DefaultModel;
         ChatEndPoint = pluginSettings.ChatEndPoint?.ToString();
         ApiKey = pluginSettings.ApiKey;
         Timeout = pluginSettings.Timeout;
      }
   }
}

using AIdentities.Connector.OpenAI.Services;

namespace AIdentities.Connector.OpenAI.Components;
public partial class Settings
{
   public class State : Shared.Features.Core.Components.BasePluginSettingsTab<OpenAISettings, State>.BaseState
   {
      public bool? Enabled { get; set; }
      public string? EndPoint { get; set; } = default!;
      public string? ApiKey { get; set; } = default!;
      public string? DefaultModel { get; set; }
      public int? Timeout { get; set; }

      public override void SetFormFields(OpenAISettings pluginSettings)
      {
         pluginSettings ??= new();
         Enabled = pluginSettings.Enabled;
         DefaultModel = pluginSettings.DefaultModel;
         EndPoint = pluginSettings.EndPoint?.ToString();
         ApiKey = pluginSettings.ApiKey;
         Timeout = pluginSettings.Timeout;
      }
   }
}

using AIdentities.Shared.Features.Core.Abstracts;

namespace AIdentities.UI.Features.Core.Components;

public partial class PluginSettingsTab
{
   class State
   {
      public IPluginSettings? CurrentPluginSettings { get; private set; }
      public Dictionary<string, object?> Parameters { get; } = new();

      public void SetCurrentPluginSettings(IPluginSettings? pluginSettings)
      {
         CurrentPluginSettings = pluginSettings;
         Parameters["PluginSettings"] = pluginSettings;
         Parameters["IsChanged"] = pluginSettings != CurrentPluginSettings;

         CurrentPluginSettings = pluginSettings;
      }
   }

   private readonly State _state = new();
}

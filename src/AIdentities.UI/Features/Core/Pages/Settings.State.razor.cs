using AIdentities.Shared.Features.Core.Abstracts;
using Microsoft.AspNetCore.Components;

namespace AIdentities.UI.Features.Core.Pages;

public partial class Settings : ComponentBase
{
   class State
   {
      public string? Message { get; set; }

      public IPluginSettings? CurrentPluginSettings { get; private set; }
      public Dictionary<string, object?> Parameters { get; } = new();

      public void SetCurrentPluginSettings(IPluginSettings? pluginSetting)
      {
         CurrentPluginSettings = pluginSetting;
         Parameters["Setting"] = pluginSetting;
         Parameters["IsChanged"] = pluginSetting != CurrentPluginSettings;

         CurrentPluginSettings = pluginSetting;
      }

   }

   private readonly State _state = new State();
}

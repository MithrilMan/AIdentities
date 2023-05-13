using AIdentities.UI.Features.Core.Services.Plugins;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace AIdentities.UI.Features.Core.Components;

public partial class PluginManagementTab : ComponentBase
{
   class State
   {
      /// <summary>
      /// List of uploaded packages.
      /// </summary>
      public List<PluginStatus> InstalledPackages { get; set; } = new();

      public bool IsDragging { get; set; } = false;

      public List<IBrowserFile> Files { get; } = new();
      public Dictionary<string, string> InvalidPackages { get; } = new();

      public PluginManifest? SelectedUploadedPackage { get; set; }
      public bool IsRemovingPackage { get; set; }
      public bool IsActivatingPackage { get; set; }
      public bool IsDisablingPackage { get; set; }
   }

   private readonly State _state = new State();
}

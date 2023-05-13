using Microsoft.AspNetCore.Components;

namespace AIdentities.UI.Features.Core.Pages;

public partial class Settings : ComponentBase
{
   /// <summary>
   /// The list of all the Plugin Settings section registered by plugins.
   /// </summary>
   [Inject] IEnumerable<PluginSettingRegistration> PluginSettingRegistration { get; set; } = null!;

   private DynamicComponent _pluginRef;

   void UndoChangesAsync()
   {

   }

   Task SaveAsync()
   {
      return Task.CompletedTask;
   }
}

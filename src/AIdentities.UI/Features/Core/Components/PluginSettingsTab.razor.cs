using AIdentities.Shared.Features.Core.Abstracts;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Components;

namespace AIdentities.UI.Features.Core.Components;

public partial class PluginSettingsTab
{
   [Inject] public IAIdentityProvider AIdentityProvider { get; set; } = default!;
   [Inject] public INotificationService NotificationService { get; set; } = default!;
   [Inject] public IPluginSettingsManager PluginSettingsManager { get; set; } = default!;

   [Parameter] public PluginSettingRegistration PluginSettingRegistration { get; set; } = default!;

   private DynamicComponent? _pluginSettingsTab;


   protected override async Task OnParametersSetAsync()
   {
      await base.OnParametersSetAsync().ConfigureAwait(false);

      var pluginSettings = await PluginSettingsManager.GetAsync(PluginSettingRegistration.PluginSettingType).ConfigureAwait(false);
      _state.SetCurrentPluginSettings(pluginSettings);
   }

   public async Task SaveAsync()
   {
      var PluginSettingTab = (IPluginSettingsTab)_pluginSettingsTab!.Instance!;
      var updatedPluginSettings = await PluginSettingTab.SaveAsync().ConfigureAwait(false);

      if (updatedPluginSettings == null)
      {
         NotificationService.ShowWarning("Please fix the errors on the form.");
         return;
      }

      await PluginSettingsManager.SetAsync(PluginSettingRegistration.PluginSettingType, updatedPluginSettings).ConfigureAwait(false);

      _state.SetCurrentPluginSettings(updatedPluginSettings);

      NotificationService.ShowSuccess("Plugin Settings updated successfully!");
   }

   public async Task UndoChangesAsync()
   {
      var PluginSettingTab = (IPluginSettingsTab)_pluginSettingsTab!.Instance!;
      await PluginSettingTab.UndoChangesAsync().ConfigureAwait(false);
   }
}

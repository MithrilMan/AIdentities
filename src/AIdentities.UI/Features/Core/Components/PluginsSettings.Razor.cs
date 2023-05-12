using AIdentities.UI.Features.Core.Services.Plugins;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace AIdentities.UI.Features.Core.Components;

public partial class PluginsSettings : ComponentBase
{
   const string VALID_FILE_TYPES = ".nupkg";
   const int MAX_FILE_NUMBER = 5;
   static readonly string[] _validFileTypes = VALID_FILE_TYPES.Split(',');

   [Inject] ILogger<PluginsSettings> Logger { get; set; } = null!;
   [Inject] IPluginManager PluginManager { get; set; } = null!;
   [Inject] INotificationService NotificationService { get; set; } = null!;
   [Inject] IDialogService DialogService { get; set; } = null!;

   protected override Task OnInitializedAsync()
   {
      return base.OnInitializedAsync();
   }

   protected override void OnInitialized()
   {
      base.OnInitialized();

      _state.InstalledPackages.AddRange(PluginManager.StoredPlugins);
      foreach (var invalidPlugin in PluginManager.InvalidPlugins)
      {
         _state.InvalidPackages.Add(invalidPlugin.Key, invalidPlugin.Value);
      }
   }

   private void OnInputFileChanged(InputFileChangeEventArgs e)
   {
      StopDragging();
      _state.Files.Clear();
      var files = e.GetMultipleFiles(MAX_FILE_NUMBER);

      foreach (var file in files)
      {
         //get file extension
         var extension = Path.GetExtension(file.Name).ToLower();

         //ensure file has valid extension
         if (!_validFileTypes.Contains(extension))
         {
            NotificationService.ShowWarning($"Invalid file extension: {file.Name} has {extension} extension, skipping file.");
            continue;
         }

         //ensure file is not already in list
         if (_state.Files.Any(f => f.Name == file.Name))
         {
            NotificationService.ShowWarning($"File {file.Name} already exists in list, skipping file.");
            continue;
         }

         _state.Files.Add(file);
      }
   }

   private async Task Clear()
   {
      _state.Files.Clear();
      StopDragging();
      await Task.Delay(100).ConfigureAwait(false);
   }

   void StopDragging() => _state.IsDragging = false;
   void StartDragging() => _state.IsDragging = true;

   void OnDragEnter() => StartDragging();
   void OnDragLeave() => StopDragging();
   void OnDragEnd() => StopDragging();



   private bool HasValidFiles()
   {
      return _state.Files.Any() && _state.Files.All(file => file.Name.EndsWith(VALID_FILE_TYPES));
   }

   void RemoveFile(MudChip closedChip)
   {
      _state.Files.Remove((IBrowserFile)closedChip.Tag!);
   }

   private async Task Install()
   {
      if (!HasValidFiles())
      {
         NotificationService.ShowError("No valid files selected.");
         return;
      }

      var manifests = new List<PluginStatus>();
      foreach (var file in _state.Files.ToList())
      {
         try
         {
            var memoryStream = new MemoryStream();
            var status = await PluginManager.StorePackageAsync(file).ConfigureAwait(false);
            manifests.Add(status);

            _state.Files.Remove(file);
         }
         catch (Exception ex)
         {
            Logger.LogError(ex, "Error installing plugin {FileName}", file.Name);
            NotificationService.ShowError($"Error installing plugin {file.Name}: {ex.Message}");
            continue;
         }
      }
      _state.InstalledPackages.AddRange(manifests);
   }

   void OnSelectedUploadedPackage(PluginManifest selectedPackage)
   {
      _state.SelectedUploadedPackage = selectedPackage;
   }

   async Task RemovePackage(PluginStatus pluginStatus)
   {
      bool? result = await DialogService.ShowMessageBox(
           "Do you want to REMOVE the plugin?",
           "Removing a package will remove it completly from the disk! Proceed?",
           yesText: "Remove it!", cancelText: "Cancel").ConfigureAwait(false);

      if (result != true) return;

      _state.IsRemovingPackage = true;
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);

      try
      {
         if (await PluginManager.RemovePackageAsync(pluginStatus.Manifest).ConfigureAwait(false))
            _state.InstalledPackages.Remove(pluginStatus);
      }
      catch (Exception ex)
      {
         Logger.LogError(ex, "Error removing package {PluginName}", pluginStatus.Manifest.Signature.GetFullName());
         NotificationService.ShowError($"Error during package removal: {ex.Message}");
      }
      finally
      {
         _state.IsRemovingPackage = false;
      }
   }

   async Task ActivatePackage(PluginStatus pluginStatus)
   {
      bool? result = await DialogService.ShowMessageBox(
           "Do you want to ACTIVATE this package?",
           "If you activate a package, you need to restart the application to take effect. Proceed?",
           yesText: "Activate it!", cancelText: "Cancel").ConfigureAwait(false);

      if (result != true) return;

      _state.IsActivatingPackage = true;
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);

      try
      {
         await PluginManager.ActivatePackageAsync(pluginStatus.Manifest).ConfigureAwait(false);
      }
      catch (Exception ex)
      {
         Logger.LogError(ex, "Error activating package {PluginName}", pluginStatus.Manifest.Signature.GetFullName());
         NotificationService.ShowError($"Error during package activation: {ex.Message}");
      }
      finally
      {
         _state.IsActivatingPackage = false;
      }
   }

   async Task DisablePackage(PluginStatus pluginStatus)
   {
      bool? result = await DialogService.ShowMessageBox(
           "Do you want to DISABLE this package?",
           "If you disable a package, you need to restart the application to take effect. Proceed?",
           yesText: "Disable it!", cancelText: "Cancel").ConfigureAwait(false);

      if (result != true) return;

      _state.IsDisablingPackage = true;
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);

      try
      {
         await PluginManager.DisablePackageAsync(pluginStatus.Manifest).ConfigureAwait(false);
      }
      catch (Exception ex)
      {
         Logger.LogError(ex, "Error removing package {PluginName}", pluginStatus.Manifest.Signature.GetFullName());
         NotificationService.ShowError($"Error during package removal: {ex.Message}");
      }
      finally
      {
         _state.IsDisablingPackage = false;
      }
   }

   async Task RemoveInvalidPackage(string packageName)
   {
      bool? result = await DialogService.ShowMessageBox(
           "Do you want to REMOVE the plugin?",
           "Removing a package will remove it completly from the disk! Proceed?",
           yesText: "Remove it!", cancelText: "Cancel").ConfigureAwait(false);

      if (result != true) return;

      try
      {
         await PluginManager.RemovePackageAsync(packageName).ConfigureAwait(false);
         _state.InvalidPackages.Remove(packageName);
      }
      catch (Exception ex)
      {
         Logger.LogError(ex, "Error removing package {PluginName}", packageName);
         NotificationService.ShowError($"Error during package removal: {ex.Message}");
      }
   }
}

using System.Text;
using AIdentities.UI.Features.AIdentityManagement.Components;
using AIdentities.UI.Features.Core.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AIdentities.UI.Features.AIdentityManagement.Pages;

[PageDefinition("AIdentities", Icons.Material.Filled.Person, "aidentities", Description = "Create and manage your set of AIdentities.")]
public partial class AIdentitiesHome : AppPage<AIdentitiesHome>
{
   /// <summary>
   /// The list of all the AIdentity feature registrations registered by plugins.
   /// </summary>
   [Inject] IEnumerable<AIdentityFeatureRegistration> AIdentityFeatureRegistrations { get; set; } = null!;
   [Inject] IEnumerable<AIdentitySafetyCheckerRegistration> AIdentitySafetyCheckerRegistrations { get; set; } = null!;
   [Inject] public IAIdentityProvider AIdentityProvider { get; set; } = default!;
   [Inject] IDialogService DialogService { get; set; } = null!;
   [Inject] IDownloadService DownloadService { get; set; } = null!;

   MudTabs? _tabs = default!;

   void CreateNewAIdentity() => StartEditing(new());

   void EditAIdentity(AIdentity aIdentity) => StartEditing(aIdentity);

   async Task DeleteAIdentity(AIdentity aIdentity)
   {
      bool result = await DialogService.OkCancelDialog(
          "DELETE AIdentity",
          "Removing an AIdentity will remove it completly from the system and my corrupt other job that has been done by this AIdentity! Proceed?",
          okText: "Remove it!",
          okColor: Color.Error).ConfigureAwait(false);

      if (result != true) return;

      foreach (var registration in AIdentitySafetyCheckerRegistrations)
      {
         var (canDelete, reasonToNotDelete) = await registration.SafetyChecker.IsAIdentitySafeToBeDeletedAsync(aIdentity).ConfigureAwait(false);
         if (canDelete == false)
         {
            NotificationService.ShowError(reasonToNotDelete ?? "", "More Details", () => GetSafetyCheckerDetails(aIdentity));
            return;
         }
      }

      Logger.LogDebug("Deleting AIdentity {AIdentity}", aIdentity);
      AIdentityProvider.Delete(aIdentity);
      if (_state.CurrentAIDentity == aIdentity)
      {
         _state.CurrentAIDentity = null;
      }
      Logger.LogDebug("AIdentity {AIdentity} deleted", aIdentity);
      _state.NeedToReload = true;
   }

   private async Task GetSafetyCheckerDetails(AIdentity aIdentity)
   {
      StringBuilder sb = new StringBuilder();
      foreach (var registration in AIdentitySafetyCheckerRegistrations)
      {
         var details = await registration.SafetyChecker.GetAIdentityActivityAsync(aIdentity).ConfigureAwait(false);
         if (details != null)
         {
            foreach (var detail in details!)
            {
               sb.AppendLine($"<b>{registration.PluginSignature.Name}</b><br>");
               sb.AppendLine($"<i>{detail.Key}</i>: {detail.Value.Description}<br>");
            }
         }
      }
      MarkupString content = new MarkupString(sb.ToString());

      await InvokeAsync(async () => await DialogService.ShowMessageBox(
       "AIdentity activities by plugin",
       content
       ).ConfigureAwait(false)).ConfigureAwait(false);
   }

   void StartEditing(AIdentity aIdentity)
   {
      _state.CurrentAIDentity = aIdentity;
      _state.ActivePanelIndex = 0;
   }

   void ExitEditing()
   {
      _state.CurrentAIDentity = null;
      _state.IsEditing = false;
   }

   async Task Import()
   {
      var dialog = await DialogService.ShowAsync<ImportAIdentity>("Import a new AIdentity", new DialogOptions()
      {
         CloseButton = true,
         CloseOnEscapeKey = true,
         Position = DialogPosition.Center,
         FullWidth = true,
      }).ConfigureAwait(false);

      var result = await dialog.Result.ConfigureAwait(false);
      if (result.Data is not AIdentity aIdentity) return;

      EditAIdentity(aIdentity);
   }

   async Task Export()
   {
      var parameters = new DialogParameters
      {
         { nameof(ExportAIdentity.AIdentity), _state.CurrentAIDentity }
      };

      var dialog = await DialogService.ShowAsync<ExportAIdentity>("Export the AIdentity", parameters, new DialogOptions()
      {
         CloseButton = true,
         CloseOnEscapeKey = true,
         Position = DialogPosition.Center,
         FullWidth = true,
      }).ConfigureAwait(false);
   }

   async Task DownloadAIdentity()
   {
      var (fileName, content) = await AIdentityProvider.GetRaw(_state.CurrentAIDentity!.Id).ConfigureAwait(false);
      using var dataStream = new MemoryStream(content);
      await DownloadService.DownloadFileFromStreamAsync(fileName, dataStream).ConfigureAwait(false);
   }

   void OnIsEditingChanged(bool isEditing)
   {
      if (!isEditing)
      {
         _state.CurrentAIDentity = null;
      }
   }
}

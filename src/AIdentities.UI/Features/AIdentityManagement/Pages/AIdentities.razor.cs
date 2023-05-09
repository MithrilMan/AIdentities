﻿using AIdentities.UI.Features.AIdentityManagement.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AIdentities.UI.Features.AIdentityManagement.Pages;

[PageDefinition("AIdentities", Icons.Material.Filled.Person, "aidentities", Description = "Create and manage your set of AIdentities.")]
public partial class AIdentities : AppPage<AIdentities>
{
   /// <summary>
   /// The list of all the AIdentity feature registrations registered by plugins.
   /// </summary>
   [Inject] IEnumerable<AIdentityFeatureRegistration> AIdentityFeatureRegistrations { get; set; } = null!;
   [Inject] public IAIdentityProvider AIdentityProvider { get; set; } = default!;
   [Inject] IDialogService DialogService { get; set; } = null!;
   [Inject] IJSRuntime JSRuntime { get; set; } = null!;

   MudTabs? _tabs = default!;

   void CreateNewAIdentity() => StartEditing(new());

   void EditAIdentity(AIdentity aIdentity) => StartEditing(aIdentity);


   void StartEditing(AIdentity aIdentity)
   {
      _state.CurrentAIDentity = aIdentity;
      _state.ActivePanelIndex = 0;
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
      using var streamRef = new DotNetStreamReference(dataStream);
      await JSRuntime.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef).ConfigureAwait(false);
   }

   void OnIsEditingChanged(bool isEditing)
   {
      if (!isEditing)
      {
         _state.CurrentAIDentity = null;
      }
   }
}

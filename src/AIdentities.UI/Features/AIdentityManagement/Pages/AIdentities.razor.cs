using System.Buffers;
using AIdentities.Chat.Components;
using AIdentities.Chat.Models;
using AIdentities.UI.Features.AIdentityManagement.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace AIdentities.UI.Features.AIdentityManagement.Pages;

[PageDefinition("AIdentities", Icons.Material.Filled.Person, "aidentities", Description = "Create and manage your set of AIdentities.")]
public partial class AIdentities : AppPage<AIdentities>
{
   [Inject] public IAIdentityProvider AIdentityProvider { get; set; } = default!;
   [Inject] IDialogService DialogService { get; set; } = null!;

   MudTabs? _tabs = default!;

   void CreateNewAIdentity() => StartEditing(new());

   void EditAIdentity(AIdentity aIdentity) => StartEditing(aIdentity);


   void StartEditing(AIdentity aIdentity)
   {
      _state.CurrentAIDentity = aIdentity;
      _state.ActivePanelIndex = 1;
   }

   async Task ImportAIdentity()
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

   void OnIsEditingChanged(bool isEditing)
   {
      if (!isEditing)
      {
         _state.CurrentAIDentity = null;
         _state.ActivePanelIndex = 0;
      }
   }
}

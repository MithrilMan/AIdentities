using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace AIdentities.UI.Features.AIdentityManagement.Components;

public partial class ImportAIdentity : ComponentBase
{
   [Inject] public INotificationService NotificationService { get; set; } = default!;
   [Inject] public IAIdentityProvider AIdentityProvider { get; set; } = default!;
   [Inject] public IEnumerable<IAIdentityImporter> AIdentityImporters { get; set; } = default!;

   [CascadingParameter] MudDialogInstance MudDialog { get; set; } = default!;

   MudForm? _form = default!;

   private Task<IEnumerable<IAIdentityImporter>> SearchImporter(string value)
   {
      var allItems = AIdentityImporters;
      if (string.IsNullOrWhiteSpace(value))
      {
         return Task.FromResult(allItems);
      }

      allItems = allItems.Where(i => i.Name?.Contains(value, StringComparison.OrdinalIgnoreCase) ?? false);

      return Task.FromResult(allItems);
   }

   private void OnImageUpload(InputFileChangeEventArgs e)
   {
      StopDragging();

      var file = e.File;
      var extension = Path.GetExtension(file.Name).ToLower();

      var hasFilters = _state.SelectedImporter?.AllowedFileExtensions?.Any() ?? false;

      if (hasFilters && !_state.SelectedImporter!.AllowedFileExtensions.Contains(extension))
      {
         NotificationService.ShowWarning($"Invalid file extension: {file.Name} has {extension} extension, cannot import it.");
         _state.File = null;

         return;
      }

      _state.File = e.File;
   }

   async Task Import()
   {
      try
      {
         var file = _state.File ?? throw new InvalidOperationException("No file was selected.");

         var aIdentity = await _state.SelectedImporter!.ImportAIdentity(file).ConfigureAwait(false);
         if (aIdentity is null)
         {
            NotificationService.ShowError($"Failed to import {file.Name}: no AIdentity was returned.");
            return;
         }

         MudDialog.Close(DialogResult.Ok(aIdentity));
      }
      catch (Exception ex)
      {
         NotificationService.ShowError($"Failed to import AIdentity: {ex.Message}");
         return;
      }

      await InvokeAsync(StateHasChanged).ConfigureAwait(false);
   }


   void Cancel() => MudDialog.Cancel();
   void StopDragging() => _state.IsDragging = false;
   void StartDragging() => _state.IsDragging = true;

}

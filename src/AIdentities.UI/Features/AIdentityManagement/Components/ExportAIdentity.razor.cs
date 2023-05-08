using Microsoft.AspNetCore.Components;

namespace AIdentities.UI.Features.AIdentityManagement.Components;

public partial class ExportAIdentity : ComponentBase
{
   const string HELP_FILENAME = "The name of the file to export the AIdentity to. The extension depends on the selected exporter.";

   [Inject] public INotificationService NotificationService { get; set; } = default!;
   [Inject] public IAIdentityProvider AIdentityProvider { get; set; } = default!;
   [Inject] public IEnumerable<IAIdentityExporter> AIdentityExporters { get; set; } = default!;

   [CascadingParameter] MudDialogInstance MudDialog { get; set; } = default!;
   [Parameter] public AIdentity AIdentity { get; set; } = default!;

   MudForm? _form = default!;

   protected override void OnInitialized()
   {
      base.OnInitialized();
      _state.SelectedExporter = AIdentityExporters.FirstOrDefault();
      _state.FileName = $"{AIdentity.Name}-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}";
   }

   private Task<IEnumerable<IAIdentityExporter>> SearchImporter(string value)
   {
      var allItems = AIdentityExporters;
      if (string.IsNullOrWhiteSpace(value))
      {
         return Task.FromResult(allItems);
      }

      allItems = allItems.Where(i => i.Name?.Contains(value, StringComparison.OrdinalIgnoreCase) ?? false);

      return Task.FromResult(allItems);
   }

   async Task Export()
   {
      if (!_form!.IsValid)
      {
         NotificationService.ShowWarning("Please fix the errors on the form before continuing.");
         return;
      }

      try
      {
         await _state.SelectedExporter!.ExportAIdentityAsync(AIdentity, _state.FileName!).ConfigureAwait(false);

         await InvokeAsync(() => MudDialog.Close()).ConfigureAwait(false);
      }
      catch (Exception ex)
      {
         NotificationService.ShowError($"Failed to export the AIdentity: {ex.Message}");
         return;
      }

      await InvokeAsync(StateHasChanged).ConfigureAwait(false);
   }

   void Cancel() => MudDialog.Cancel();
}

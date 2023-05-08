using System.Buffers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace AIdentities.UI.Features.AIdentityManagement.Components;

public partial class TabGeneric
{
   const string VALID_FILE_TYPES = ".jpeg,.jpg,.png,.gif";

   const string HELP_NAME = @"The name of the AIdentity.";

   const string HELP_DESCRIPTION = @"The description of the AIdentity.
It's used to give a brief description of the AIdentity.";

   const string HELP_PERSONALITY = @"The AIdentity's personality.
The usage depends on the feature using the AIdentity.
";

   [Inject] protected INotificationService NotificationService { get; set; } = default!;
   [Inject] public IAIdentityProvider AIdentityProvider { get; set; } = default!;

   [Parameter] public AIdentity? AIdentity { get; set; }
   [Parameter] public EventCallback<AIdentity?> AIdentityChanged { get; set; }
   [Parameter] public bool IsEditing { get; set; }
   [Parameter] public EventCallback<bool> IsEditingChanged { get; set; }

   MudForm? _form;

   protected override void OnInitialized() => base.OnInitialized();

   protected override void OnParametersSet()
   {
      base.OnParametersSet();
      _state.IsEditing = IsEditing;
      _state.SetFormFields(AIdentity);
   }

   private async Task OnImageUpload(InputFileChangeEventArgs e)
   {
      StopDragging();

      var file = e.File;
      var extension = Path.GetExtension(file.Name).ToLower();

      if (!VALID_FILE_TYPES.Split(',').Contains(extension))
      {
         NotificationService.ShowWarning($"Invalid file extension: {file.Name} has {extension} extension, skipping file.");
         return;
      }

      //file = await file.RequestImageFileAsync(file.ContentType, 512, 512).ConfigureAwait(false);

      byte[] buffer = ArrayPool<byte>.Shared.Rent((int)e.File.Size);
      try
      {
         using var openedStream = file.OpenReadStream(5 * 1000 * 1024); // 5MB max
         using var memoryStream = new MemoryStream(buffer);
         await openedStream.CopyToAsync(memoryStream).ConfigureAwait(false);
         int dataLength = (int)memoryStream.Length;
         if (dataLength > 0)
         {
            string base64Data = Convert.ToBase64String(buffer);
            _state.Image = $"data:{file.ContentType};base64,{base64Data}";
         }
         else
         {
            _state.Image = null;
         }
      }
      finally
      {
         ArrayPool<byte>.Shared.Return(buffer);
      }
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);
   }

   void OnUndo()
   {
      _state.SetFormFields(null);
      IsEditingChanged.InvokeAsync(false);
   }

   async Task OnSave()
   {
      await _form!.Validate().ConfigureAwait(false);
      if (!_form.IsValid)
      {
         NotificationService.ShowWarning("Please fix the errors on the form.");
         return;
      }

      var modifiedAIdentity = AIdentity! with
      {
         Name = _state.Name!,
         Description = _state.Description!,
         Image = _state.Image!,
         Personality = _state.Personality,
      };

      AIdentityProvider.Update(modifiedAIdentity);
      NotificationService.ShowSuccess("AIdentity updated successfully!");
      await AIdentityChanged.InvokeAsync(modifiedAIdentity).ConfigureAwait(false);
   }

   void StopDragging() => _state.IsDragging = false;
   void StartDragging() => _state.IsDragging = true;
}

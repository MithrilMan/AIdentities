using System.Buffers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Entities = AIdentities.Shared.Features.Core;

namespace AIdentities.UI.Features.AIdentity.Pages;

[PageDefinition("AIdentities", Icons.Material.Filled.Person, "aidentities", Description = "Create and manage your set of AIdentities.")]
public partial class AIdentities : AppPage<AIdentities>
{
   const string VALID_FILE_TYPES = ".jpeg,.jpg,.png,.gif";

   const string HELP_NAME = @"The name of the AIdentity.";

   const string HELP_BACKGROUND = @"AIdentity's background.
You can for example specify where the AIdentity is from, or what it does for a living.";

   const string HELP_DESCRIPTION = @"The description of the AIdentity.
It has no impact on how it responds, it's used only to give a brief description of the AIdentity.";

   const string HELP_FULL_PROMPT = @"The full prompt passed to the LLM to start the conversation.
When specified, the LLM will use this prompt to start the conversation.
";
   const string HELP_FULL_PERSONALITY = @"The AIdentity's personality.
This is injected in the LLM prompt to make the AIdentity behave following a specific personality.
";
   const string HELP_FIRST_MESSAGE = @"The first message sent by the AIdentity when a new conversation starts.
It has no impact on how it responds, It's purely cosmetic.";

   [Inject] public IAIdentityProvider AIdentityProvider { get; set; } = default!;

   MudForm? _form;

   void OnUndo()
   {
      _state.CurrentAIDentity = null;
      _state.SetFormFields(null);
   }

   async Task OnSave()
   {
      await _form!.Validate().ConfigureAwait(false);
      if (!_form.IsValid)
      {
         NotificationService.ShowWarning("Please fix the errors on the form.");
         return;
      }

      var modifiedAIdentity = _state.CurrentAIDentity with
      {
         Name = _state.Name!,
         Description = _state.Description!,
         Image = _state.Image!,
         Background = _state.Background,
         FullPrompt = _state.FullPrompt!,
         Personality = _state.Personality,
         FirstMessage = _state.FirstMessage
      };

      AIdentityProvider.Update(modifiedAIdentity);
      NotificationService.ShowSuccess("AIdentity updated successfully!");
   }

   void CreateNewAIdentity()
   {
      _state.CurrentAIDentity = new();
      _state.SetFormFields(_state.CurrentAIDentity);
   }

   void EditAIdentity(Entities.AIdentity aIdentity)
   {
      _state.CurrentAIDentity = aIdentity;
      _state.SetFormFields(aIdentity);
   }

   private async void OnImageUpload(InputFileChangeEventArgs e)
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

   void StopDragging() => _state.IsDragging = false;
   void StartDragging() => _state.IsDragging = true;
   void OnDragEnter() => StartDragging();
   void OnDragLeave() => StopDragging();
   void OnDragEnd() => StopDragging();
}

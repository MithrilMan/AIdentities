using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;

namespace AIdentities.UI.Features.Core.Components;

public partial class DragAndDropFileUpload
{
   [Parameter] public bool IsDisabled { get; set; }
   [Parameter] public string? Accept { get; set; }
   [Parameter] public EventCallback<InputFileChangeEventArgs> OnFilesChanged { get; set; }
   [Parameter] public int MaximumFileCount { get; set; } = 1;
   [Parameter] public string DropZoneText { get; set; } = "Click or Drop files here";
   [Parameter] public RenderFragment? DropZoneTemplate { get; set; }

   bool _isDragging = false;


   void StopDragging() => _isDragging = false;
   void StartDragging() => _isDragging = true;

   Task FilesChanged(InputFileChangeEventArgs e)
   {
      StopDragging();
      return OnFilesChanged.InvokeAsync(e);
   }
}

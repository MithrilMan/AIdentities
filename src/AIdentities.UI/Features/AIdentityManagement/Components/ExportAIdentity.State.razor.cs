namespace AIdentities.UI.Features.AIdentityManagement.Components;

public partial class ExportAIdentity
{
   class State
   {
      public string? FileName { get; set; }
      public IAIdentityExporter? SelectedExporter { get; set; }
      public bool IsEditing => SelectedExporter != null;
   }

   private readonly State _state = new();
}

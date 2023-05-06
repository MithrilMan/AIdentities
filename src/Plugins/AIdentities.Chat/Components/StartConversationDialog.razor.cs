using MudBlazor;

namespace AIdentities.Chat.Components;

public partial class StartConversationDialog : ComponentBase
{
   [Inject] public IChatStorage ChatStorage { get; set; } = default!;
   [Inject] public INotificationService NotificationService { get; set; } = default!;
   [Inject] public IAIdentityProvider AIdentityProvider { get; set; } = default!;

   [CascadingParameter] MudDialogInstance MudDialog { get; set; } = default!;

   MudForm? _form = default!;

   //generate random AIdentities
   List<AIdentity> AIdentities { get; set; } = GenerateRandomAIdentities();

   void Cancel() => MudDialog.Cancel();

   private static List<AIdentity> GenerateRandomAIdentities()
   {
      var list = new List<AIdentity>();
      var dataUri = "data:image/png;base64,iVBORw0KGgoAAA\r\nANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4\r\n//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU\r\n5ErkJggg==";
      for (var i = 0; i < 10; i++)
      {
         list.Add(new AIdentity()
         {
            Id = Guid.NewGuid(),
            Name = $"AIdentity {i}",
            Image = dataUri
         });
      }

      return list;
   }


   async Task StartConversation()
   {
      await _form.Validate().ConfigureAwait(false);
      if (!_form.IsValid)
      {
         NotificationService.ShowWarning("Please fix the errors on the form.");
         return;
      }

      var conversation = new Conversation();
      conversation.Metadata = new()
      {
         ConversationId = conversation.Id,
         AIdentityId = _state.SelectedAIdentity!.Id,
         Title = "New Conversation"
      };

      MudDialog.Close(DialogResult.Ok(conversation));
   }
}

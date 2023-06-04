namespace AIdentities.Chat.Components;

public partial class StartConversationDialog : ComponentBase
{
   [Inject] public INotificationService NotificationService { get; set; } = default!;
   [Inject] public IAIdentityProvider AIdentityProvider { get; set; } = default!;

   [CascadingParameter] MudDialogInstance MudDialog { get; set; } = default!;

   MudForm? _form = default!;

   void Cancel() => MudDialog.Cancel();

   async Task StartConversation()
   {
      await _form!.Validate().ConfigureAwait(false);
      if (!_form.IsValid)
      {
         NotificationService.ShowWarning("Please fix the errors on the form.");
         return;
      }

      var conversation = new Conversation("New Conversation");
      conversation.AddAIdentity(_state.SelectedAIdentity!);

      MudDialog.Close(DialogResult.Ok(conversation));
   }
}

namespace AIdentities.Chat.Components;

public partial class StartChatDialog : ComponentBase
{
   [Inject] public IChatStorage ChatStorage { get; set; } = default!;
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

      var conversation = new ChatBlock();
      conversation.Metadata = new()
      {
         ConversationId = conversation.Id,
         Title = "New Conversation"
      };
      conversation.Metadata.AIdentityIds.Add(_state.SelectedAIdentity!.Id);

      MudDialog.Close(DialogResult.Ok(conversation));
   }
}

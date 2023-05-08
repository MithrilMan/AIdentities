using System.Reflection.Metadata.Ecma335;
using MudBlazor;

namespace AIdentities.Chat.Components;
public partial class TabAIdentityFeatureChat : IAIdentityFeatureTab<AIdentityChatFeature>
{
   const string HELP_BACKGROUND = @"AIdentity's background.
You can for example specify where the AIdentity is from, or what it does for a living.";

   const string HELP_FULL_PROMPT = @"The full prompt passed to the LLM to start the conversation.
When specified, the LLM will use this prompt to start the conversation.
";
   const string HELP_FIRST_MESSAGE = @"The first message sent by the AIdentity when a new conversation starts.
It has no impact on how it responds, It's purely cosmetic.";

   [Inject] protected INotificationService NotificationService { get; set; } = default!;
   [Inject] public IAIdentityProvider AIdentityProvider { get; set; } = default!;

   [Parameter] public AIdentityChatFeature Feature { get; set; } = default!;

   MudForm? _form;

   protected override void OnInitialized() => base.OnInitialized();

   protected override void OnParametersSet()
   {
      base.OnParametersSet();
      _state.SetFormFields(Feature);
   }

   public async Task<AIdentityChatFeature?> SaveAsync()
   {
      await _form!.Validate().ConfigureAwait(false);
      if (!_form.IsValid)
      {
         return null;
      }

      // care about this expression, parenthesis are important
      return (Feature ?? new AIdentityChatFeature()) with
      {
         Background = _state.Background,
         FullPrompt = _state.FullPrompt!,
         FirstMessage = _state.FirstMessage,
         UseFullPrompt = _state.UseFullPrompt
      };
   }

   public Task UndoChangesAsync()
   {
      _state.SetFormFields(Feature);
      return Task.CompletedTask;
   }

   async Task<object?> IAIdentityFeatureTab.SaveAsync()
   {
      var result = await SaveAsync().ConfigureAwait(false);
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);
      return result;
   }
}

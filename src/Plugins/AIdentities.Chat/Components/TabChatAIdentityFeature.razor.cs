﻿namespace AIdentities.Chat.Components;
public partial class TabChatAIdentityFeature : IAIdentityFeatureTab<AIdentityChatFeature>
{
   const int MAX_EXAMPLES = 5;

   const string HELP_BACKGROUND = @"AIdentity's background.
You can for example specify where the AIdentity is from, or what it does for a living.";

   const string HELP_FULL_PROMPT = @"The full prompt passed to the LLM to start the conversation.
When specified, the LLM will use this prompt to start the conversation.
";

   [Inject] protected INotificationService NotificationService { get; set; } = default!;
   [Inject] public IAIdentityProvider AIdentityProvider { get; set; } = default!;

   [Parameter] public bool IsChanged { get; set; } = default!;
   [Parameter] public AIdentity AIdentity { get; set; } = default!;
   [Parameter] public AIdentityChatFeature Feature { get; set; } = default!;

   MudForm? _form;

   protected override void OnInitialized()
   {
      base.OnInitialized();
      _state.SetFormFields(Feature);
   }

   protected override void OnParametersSet()
   {
      base.OnParametersSet();
      if (IsChanged)
      {
         _state.SetFormFields(Feature);
      }
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
         UseFullPrompt = _state.UseFullPrompt,
         ExampleMessages = _state.ExampleMessages,
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

   void AddExample()
   {
      _state.ExampleMessages.Add(new AIdentityUserExchange());
   }
}

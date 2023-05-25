namespace AIdentities.Shared.Features.CognitiveEngine.Components;

public partial class TabAIdentityFeatureSkills : IAIdentityFeatureTab<AIdentityFeatureSkills>
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

   [Parameter] public bool IsChanged { get; set; } = default!;
   [Parameter] public AIdentityFeatureSkills Feature { get; set; } = default!;

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
         _state.SetFormFields(Feature);
   }

   public async Task<AIdentityFeatureSkills?> SaveAsync()
   {
      await _form!.Validate().ConfigureAwait(false);
      if (!_form.IsValid)
         return null;

      // care about this expression, parenthesis are important
      return (Feature ?? new AIdentityFeatureSkills()) with
      {
         AreSkillsEnabled = _state.AreSkillsEnabled,
         EnabledSkills = _state.EnabledSkills.ToList(),
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

namespace AIdentities.Shared.Features.CognitiveEngine.Components;

public partial class TabAIdentityFeatureSkills : IAIdentityFeatureTab<AIdentityFeatureSkills>
{
   [Inject] protected INotificationService NotificationService { get; set; } = default!;
   [Inject] public IAIdentityProvider AIdentityProvider { get; set; } = default!;

   [Parameter] public bool IsChanged { get; set; } = default!;
   [Parameter] public AIdentity AIdentity { get; set; } = default!;
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

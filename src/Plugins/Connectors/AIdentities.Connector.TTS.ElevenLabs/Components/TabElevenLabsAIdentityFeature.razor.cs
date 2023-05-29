namespace AIdentities.Connector.TTS.ElevenLabs.Components;

public partial class TabElevenLabsAIdentityFeature : IAIdentityFeatureTab<ElevenLabsAIdentityFeature>
{
   [Inject] protected INotificationService NotificationService { get; set; } = default!;
   [Inject] public IAIdentityProvider AIdentityProvider { get; set; } = default!;

   [Parameter] public bool IsChanged { get; set; } = default!;
   [Parameter] public ElevenLabsAIdentityFeature Feature { get; set; } = default!;

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

   public async Task<ElevenLabsAIdentityFeature?> SaveAsync()
   {
      await _form!.Validate().ConfigureAwait(false);
      if (!_form.IsValid)
         return null;

      // care about this expression, parenthesis are important
      return (Feature ?? new ElevenLabsAIdentityFeature()) with
      {
         Customize = _state.Customize ?? ElevenLabsAIdentityFeature.DEFAULT_CUSTOMIZE,
         ModelId = _state.ModelId,
         VoiceId = _state.VoiceId,
         VoiceStability = _state.VoiceStability,
         VoiceSimilarityBoost = _state.VoiceSimilarityBoost,
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

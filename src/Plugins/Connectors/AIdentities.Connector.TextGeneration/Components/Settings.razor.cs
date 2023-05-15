using AIdentities.Shared.Features.Core.Components;
using MudBlazor;

namespace AIdentities.Connector.TextGeneration.Components;
public partial class Settings : BasePluginSettingsTab<TextGenerationSettings, Settings.State>
{
   MudForm? _form;

   protected override async ValueTask<bool> AreSettingsValid()
   {
      await _form!.Validate().ConfigureAwait(false);
      return _form.IsValid;
   }

   public override Task<TextGenerationSettings> PerformSavingAsync()
   {
      return Task.FromResult(new TextGenerationSettings()
      {
         DefaultModel = _state.DefaultModel ?? TextGenerationSettings.DEFAULT_MODEL,
         Enabled = _state.Enabled ?? TextGenerationSettings.DEFAULT_ENABLED,
         CompletionEndPoint = new Uri(_state.CompletionEndPoint!),
         StreamedCompletionEndPoint = new Uri(_state.StreamedCompletionEndPoint!),
         Timeout = _state.Timeout ?? TextGenerationSettings.DEFAULT_TIMEOUT
      });
   }
}

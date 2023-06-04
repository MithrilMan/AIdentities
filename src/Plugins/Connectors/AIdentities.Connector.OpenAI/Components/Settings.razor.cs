using AIdentities.Connector.OpenAI.Models;
using AIdentities.Shared.Features.Core.Components;
using MudBlazor;

namespace AIdentities.Connector.OpenAI.Components;
public partial class Settings : BasePluginSettingsTab<OpenAISettings, Settings.State>
{
   MudForm? _form;

   protected override async ValueTask<bool> AreSettingsValid()
   {
      await _form!.Validate().ConfigureAwait(false);
      return _form.IsValid;
   }

   public override Task<OpenAISettings> PerformSavingAsync()
   {
      return Task.FromResult(new OpenAISettings()
      {
         ApiKey = _state.ApiKey,
         DefaultChatModel = _state.DefaultChatModel ?? OpenAISettings.DEFAULT_CHAT_MODEL,
         Enabled = _state.Enabled,
         ChatEndPoint = new Uri(_state.ChatEndPoint!),
         CompletionEndPoint = new Uri(_state.CompletionEndPoint!),
         Timeout = _state.Timeout ?? OpenAISettings.DEFAULT_TIMEOUT
      });
   }
}

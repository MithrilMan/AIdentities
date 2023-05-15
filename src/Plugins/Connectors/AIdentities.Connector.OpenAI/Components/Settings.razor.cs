﻿using AIdentities.Connector.OpenAI.Models;
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
         DefaultModel = _state.DefaultModel ?? OpenAISettings.DEFAULT_MODEL,
         Enabled = _state.Enabled ?? OpenAISettings.DEFAULT_ENABLED,
         ChatEndPoint = new Uri(_state.ChatEndPoint!),
         Timeout = _state.Timeout ?? OpenAISettings.DEFAULT_TIMEOUT
      });
   }
}

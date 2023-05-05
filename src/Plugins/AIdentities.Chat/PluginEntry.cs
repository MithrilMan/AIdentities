﻿using AIdentities.Chat.Components;
using AIdentities.Chat.Extendability;
using AIdentities.Chat.Services.Connectors.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AIdentities.Chat;
public class PluginEntry : BasePluginEntry
{
   public override void RegisterServices(IServiceCollection services)
   {
      services.AddScoped<IChatConnector, OpenAIConnector>();
      services.AddScoped<IChatStorage>(sp => new ChatStorage(
         logger: sp.GetRequiredService<ILogger<ChatStorage>>(),
         pluginStorage: _storage
         ));

      services
         .AddSingleton<IValidateOptions<OpenAIOptions>, OpenAIOptionsValidator>()
         .AddOptions<OpenAIOptions>()
         .BindConfiguration(OpenAIOptions.SECTION_NAME)
         .ValidateOnStart();

      // Register the AIdentity feature to expose an editor in the AIdentity management page.
      RegisterFeature<AIdentityChatFeature, TabAIdentityFeatureChat>("Chat");
   }
}
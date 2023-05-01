﻿using AIdentities.Chat.Extendability;
using AIdentities.Chat.Services.Connectors.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AIdentities.Chat;
public class ChatPlugin : IPluginEntry
{
   private PluginManifest _manifest = default!;
   private IPluginStorage _storage = default!;

   //public IEnumerable<Type> Pages => throw new NotImplementedException();

   //public IEnumerable<Type> ModelConnectors { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

   public void Initialize(PluginManifest manifest, IServiceCollection services, IPluginStorage pluginStorage)
   {
      _manifest = manifest;
      _storage = pluginStorage;

      RegisterServices(services);
   }

   public void RegisterServices(IServiceCollection services)
   {
      services.AddScoped<IChatConnector, OpenAIConnector>();
      services.AddScoped<IChatStorage>(sp => new ChatStorage(
         logger: sp.GetRequiredService<ILogger<ChatStorage>>(),
         pluginStorage: _storage
         ));

      services.AddOptions<OpenAIOptions>()
         .BindConfiguration(OpenAIOptions.SECTION_NAME)
         .Validate<OpenAIOptionsValidator>((o, validator) => validator.Validate(o))
         .ValidateOnStart();
   }
}

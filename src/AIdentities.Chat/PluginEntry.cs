using AIdentities.Chat.Extendability;
using AIdentities.Chat.Services.Connectors.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AIdentities.Chat;
public class PluginEntry : IPluginEntry
{
   private PluginManifest _manifest = default!;
   private IPluginStorage _storage = default!;

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

      services
         .AddSingleton<IValidateOptions<OpenAIOptions>, OpenAIOptionsValidator>()
         .AddOptions<OpenAIOptions>()
         .BindConfiguration(OpenAIOptions.SECTION_NAME)
         .ValidateOnStart();
   }
}

using AIdentities.Chat.Components;
using Microsoft.Extensions.DependencyInjection;

namespace AIdentities.Chat;
public class PluginEntry : BasePluginEntry
{
   public override void RegisterServices(IServiceCollection services)
   {
      services.AddScoped<IChatStorage>(sp => new ChatStorage(
         logger: sp.GetRequiredService<ILogger<ChatStorage>>(),
         pluginStorage: _storage
         ));

      services
         .AddTransient<IChatPromptGenerator, ChatPromptGenerator>();

      // Register the AIdentity feature to expose an editor in the AIdentity management page.
      RegisterAIdentityFeature<AIdentityChatFeature, TabAIdentityFeatureChat>("Chat");

      RegisterAIdentitySafetyChecker<ChatAIdentitySafetyChecker>();
   }
}

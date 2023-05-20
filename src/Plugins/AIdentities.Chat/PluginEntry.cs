using AIdentities.Chat.Components;
using Microsoft.Extensions.DependencyInjection;

namespace AIdentities.Chat;
public class PluginEntry : BasePluginEntry<PluginEntry>
{
   public override void RegisterServices(IServiceCollection services)
   {
      services
         .AddScoped<IChatStorage, ChatStorage>()
         .AddScoped<IConversationExporter, ConversationExporter>();

      services
         .AddTransient<IChatPromptGenerator, ChatPromptGenerator>();

      // Register the AIdentity feature to expose an editor in the AIdentity management page.
      RegisterAIdentityFeature<AIdentityChatFeature, TabAIdentityFeatureChat>("Chat");

      RegisterAIdentitySafetyChecker<ChatAIdentitySafetyChecker>();

      RegisterPluginSettings<ChatSettings, Settings>("Chat");
   }
}

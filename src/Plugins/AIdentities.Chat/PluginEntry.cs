using AIdentities.Chat.Components;
using AIdentities.Chat.Skills.CallAFriend;
using AIdentities.Shared.Features.CognitiveEngine.Skills;
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
      RegisterAvailableCommands(services);
   }

   private void RegisterAvailableCommands(IServiceCollection services)
   {
      services
         .AddScoped<ISkillAction, InviteFriend>();
   }
}

using AIdentities.Chat.Components;
using AIdentities.Chat.Skills.IntroduceYourself;
using AIdentities.Chat.Skills.InviteFriend;
using AIdentities.Chat.Skills.ReplyToPrompt;
using Microsoft.Extensions.DependencyInjection;

namespace AIdentities.Chat;
public class PluginEntry : BasePluginEntry<PluginEntry>
{
   public override void RegisterServices(IServiceCollection services)
   {
      services
         .AddScoped<IChatStorage, ChatStorage>()
         .AddScoped<IConversationExporter, ConversationExporter>();

      services.AddTransient<IChatPromptGenerator, ChatPromptGenerator>();

      services.AddTransient<CognitiveChatMission>();

      // Register the AIdentity feature to expose an editor in the AIdentity management page.
      RegisterAIdentityFeature<AIdentityChatFeature, TabAIdentityFeatureChat>("Chat");

      RegisterAIdentitySafetyChecker<ChatAIdentitySafetyChecker>();

      RegisterPluginSettings<ChatSettings, Settings>("Chat");
      RegisterAvailableCommands(services);
   }

   private static void RegisterAvailableCommands(IServiceCollection services)
   {
      services
         .AddScoped<ISkillAction, InviteFriend>()
         .AddScoped<ISkillAction, IntroduceYourself>()
         .AddScoped<ISkillAction, ReplyToPrompt>()
         ;
   }
}

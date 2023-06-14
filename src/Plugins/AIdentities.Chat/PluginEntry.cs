using AIdentities.Chat.CognitiveEngine;
using AIdentities.Chat.Components;
using AIdentities.Chat.Persistence;
using AIdentities.Chat.Skills.CreateStableDiffusionPrompt;
using AIdentities.Chat.Skills.IntroduceYourself;
using AIdentities.Chat.Skills.InviteToChat;
using AIdentities.Chat.Skills.ReplyToPrompt;
using Microsoft.Extensions.DependencyInjection;

namespace AIdentities.Chat;
public class PluginEntry : BasePluginEntry<PluginEntry>
{
   public override void RegisterServices(IServiceCollection services)
   {
      services
         .AddScoped<IConversationExporter, ConversationExporter>();

      services
         .AddScoped<ICognitiveChatStorage, CognitiveChatStorage>();

      services
         .AddTransient<CognitiveChatMission>()
         .AddScoped<ICognitiveEngineFactory, ChatKeeperCognitiveEngineFactory>()
         .AddScoped<ICognitiveEngineFactory, ChatCognitiveEngineFactory>();

      services.AddDbContext<ConversationDbContext>();

      // Register the AIdentity feature to expose an editor in the AIdentity management page.
      RegisterAIdentityFeature<AIdentityChatFeature, TabChatAIdentityFeature>("Chat");
      RegisterAIdentitySafetyChecker<ChatAIdentitySafetyChecker>();
      RegisterPluginStartup<StartupService>();
      RegisterPluginSettings<ChatSettings, Settings>("Chat");
      RegisterAvailableCommands(services);
   }

   private static void RegisterAvailableCommands(IServiceCollection services)
   {
      services
         .AddScoped<ISkill, InviteToChat>()
         .AddScoped<ISkill, IntroduceYourself>()
         .AddScoped<ISkill, ReplyToPrompt>()
         .AddScoped<ISkill, CreateStableDiffusionPrompt>()
         ;
   }
}

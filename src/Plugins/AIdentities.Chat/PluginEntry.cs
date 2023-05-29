﻿using AIdentities.Chat.CognitiveEngine;
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
         .AddScoped<IChatExporter, ChatExporter>();

      services
         .AddScoped<ICognitiveChatStorage, CognitiveChatStorage>();

      services.AddTransient<IChatPromptGenerator, ChatPromptGenerator>();

      services
         .AddTransient<CognitiveChatMission>()
         .AddScoped<ICognitiveEngineFactory, ChatKeeperCognitiveEngineFactory>()
         .AddScoped<ICognitiveEngineFactory, ChatCognitiveEngineFactory>();


      // Register the AIdentity feature to expose an editor in the AIdentity management page.
      RegisterAIdentityFeature<AIdentityChatFeature, TabChatAIdentityFeature>("Chat");

      RegisterAIdentitySafetyChecker<ChatAIdentitySafetyChecker>();

      RegisterPluginSettings<ChatSettings, Settings>("Chat");
      RegisterAvailableCommands(services);
   }

   private static void RegisterAvailableCommands(IServiceCollection services)
   {
      services
         .AddScoped<ISkill, InviteFriend>()
         .AddScoped<ISkill, IntroduceYourself>()
         .AddScoped<ISkill, ReplyToPrompt>()
         ;
   }
}

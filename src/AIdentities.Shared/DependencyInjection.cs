using AIdentities.Shared.Features.AIdentities.Services;
using AIdentities.Shared.Features.CognitiveEngine;
using AIdentities.Shared.Features.CognitiveEngine.Components;
using AIdentities.Shared.Features.CognitiveEngine.Engines.Reflexive;
using AIdentities.Shared.Features.CognitiveEngine.Memory.Conversation;
using AIdentities.Shared.Features.Core.Services;
using AIdentities.Shared.Plugins.Connectors;
using AIdentities.Shared.Services.EventBus;
using Microsoft.Extensions.DependencyInjection;

namespace AIdentities.Shared;

public static class DependencyInjection
{
   public static IServiceCollection AddSharedServices(this IServiceCollection services)
   {
      services.AddScoped<IAppComponentSettingsManager, AppComponentSettingsManager>();

      services.AddScoped<IPluginSettingsManager, PluginSettingsManager>();
      services.AddScoped<IPluginResourcePath, PluginResourcePath>();

      services
         .AddScoped<EventAggregator.Blazor.IEventAggregator, EventAggregator.Blazor.EventAggregator>()
         .AddScoped<IEventBus, EventBus>();

      services
         .AddScoped<IAIdentityProvider, AIdentityProvider>()
         .AddScoped<AIdentityProviderSerializationSettings>();

      services
         .AddScoped<ISkillManager, SkillManager>();

      services.AddScoped(typeof(IConnectorsManager<>), typeof(ConnectorsManager<>));

      RegisterCogntiveEngines(services);

      return services;
   }

   /// <summary>
   /// Registers the cognitive engines related services.
   /// </summary>
   /// <param name="services"></param>
   private static void RegisterCogntiveEngines(IServiceCollection services)
   {
      // this service has to be used by whoever wants to create a cognitive engine for a specific AIdentity.
      // it will make use of the various factories to create the proper cognitive engine.
      services.AddScoped<ICognitiveEngineProvider, CognitiveEngineProvider>();

      services
         .AddScoped<ICognitiveEngineFactory, DefaultReflexiveCognitiveEngineFactory>();

      // register the AIdentity skills feature and its tab to allow users
      // to enable or disable skills for an AIdentity and customize the settings for each skill that have customizable settings.
      services.AddSingleton(new AIdentityFeatureRegistration(typeof(AIdentityFeatureSkills), typeof(TabAIdentityFeatureSkills), "Skills"));

      // Probably would be better to have ConversationHistory instantiated by a factory so a consumer
      // can inject the factory and instantiate how many history it wants.
      services.AddTransient<IConversationHistory, ConversationHistory>();
   }
}

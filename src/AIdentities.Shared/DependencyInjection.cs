using AIdentities.Shared.Features.AIdentities.Services;
using AIdentities.Shared.Features.CognitiveEngine;
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

      services.AddScoped<IDefaultConnectors, DefaultConnectors>();

      RegisterCogntiveEngines(services);

      return services;
   }

   private static void RegisterCogntiveEngines(IServiceCollection services)
   {
      // this service has to be used by whoever wants to create a cognitive engine for a specific AIdentity.
      // it will make use of the various factories to create the proper cognitive engine.
      services.AddScoped<ICognitiveEngineProvider, CognitiveEngineProvider>();

      services
         .AddScoped<ICognitiveEngineFactory, MithrilCognitiveEngineFactory>();
   }
}

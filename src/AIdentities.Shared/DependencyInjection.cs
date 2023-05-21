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

      services
         .AddScoped<EventAggregator.Blazor.IEventAggregator, EventAggregator.Blazor.EventAggregator>()
         .AddScoped<IEventBus, EventBus>();

      services
         .AddScoped<IAIdentityProvider, AIdentityProvider>()
         .AddScoped<AIdentityProviderSerializationSettings>();

      services
         .AddScoped<ICognitiveEngineProvider, CognitiveEngineProvider>()
         .AddScoped<ISkillActionsManager, SkillActionsManager>();

      services.AddScoped<IDefaultConnectors, DefaultConnectors>();

      return services;
   }
}

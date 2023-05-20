using AIdentities.Shared.Features.AIdentities.Services;
using AIdentities.Shared.Features.Core.Services;
using AIdentities.Shared.Plugins.Storage;
using AIdentities.Shared.Services.EventBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using MudExtensions.Services;
using Toolbelt.Blazor.Extensions.DependencyInjection;

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

      return services;
   }
}

using AIdentities.Shared.Plugins.Storage;
using AIdentities.UI.Features.AIdentity.Services;
using AIdentities.UI.Features.Core.Services.PageManager;
using Microsoft.Extensions.Options;

namespace AIdentities.UI;

public static class DependencyInjection
{
   public static IServiceCollection AddAIdentitiesServices(
      this IServiceCollection services,
      IWebHostEnvironment webHostEnvironment)
   {
      services.AddCors(options =>
      {
         options.AddPolicy("CorsPolicy", builder =>
         builder
         .AllowAnyOrigin()
         .AllowAnyMethod()
         .AllowAnyHeader());
      });

      // Add services to the container.
      services.AddRazorPages(options => options.RootDirectory = "/Features/Core/Pages");
      services.AddServerSideBlazor();

      services.AddSingleton<AppOptionsValidator>();
      services.AddOptions<AppOptions>()
         .BindConfiguration(AppOptions.SECTION_NAME)
         .Validate<AppOptionsValidator>((o, validator) => validator.Validate(o))
         .ValidateOnStart();

      services.AddMudServices();
      services.AddBlazoredLocalStorage();

      services.AddScoped<IPageDefinitionProvider, PageDefinitionProvider>();
      services.AddScoped<INotificationService, NotificationService>();
      services.AddScoped<IAppComponentSettingsManager, AppComponentSettingsManager>();
      services.AddScoped<EventAggregator.Blazor.IEventAggregator, EventAggregator.Blazor.EventAggregator>();
      services.AddScoped<IEventBus, EventBus>();
      services.AddSingleton<IAIdentityProvider, AIdentityProvider>();

      RegisterPlugins(services, webHostEnvironment);

      RegisterDebuggableModules(services, webHostEnvironment);

      return services;
   }

   private static void RegisterPlugins(IServiceCollection services, IWebHostEnvironment webHostEnvironment)
   {
      services.AddSingleton<IPluginStorageFactory, PluginStorageFactory>();
      services.AddSingleton<IPackageInspector, PackageInspector>();

      // In order to let plugins register their services, we need to load packages before we can build the service provider.
      // At the moment to not duplicate code, we create a new instance of the plugin manager here in order to load the plugin assemblies.

      var sp = services.BuildServiceProvider();
      var pluginManager = new PluginManager(
         logger: sp.GetRequiredService<ILogger<PluginManager>>(),
         options: sp.GetRequiredService<IOptions<AppOptions>>(),
         webHostEnvironment: webHostEnvironment,
         pluginStorageFactory: sp.GetRequiredService<IPluginStorageFactory>(),
         packageInspector: sp.GetRequiredService<IPackageInspector>()
         );

      pluginManager.LoadStoredPackagesAsync(services).GetAwaiter().GetResult();

      services
         .AddSingleton<IPluginManager, PluginManager>(sp =>
         {
            pluginManager.SwapDependencies(
               logger: sp.GetRequiredService<ILogger<PluginManager>>(),
               options: sp.GetRequiredService<IOptions<AppOptions>>(),
               pluginStorageFactory: sp.GetRequiredService<IPluginStorageFactory>(),
               packageInspector: sp.GetRequiredService<IPackageInspector>()
               );

            return pluginManager;
         });
   }

   private static void RegisterDebuggableModules(IServiceCollection services, IWebHostEnvironment webHostEnvironment)
   {
      services.AddSingleton<IDebuggablePagesManager, DebuggablePagesManager>();
      // in order to let debuggable modules register their services, we need to register them.
      services.AddSingleton<IDebuggableModule, Chat.DebuggableModule>();


      //perform a first pass in order to register the services properly.
      var sp = services.BuildServiceProvider();
      var manager = sp.GetRequiredService<IDebuggablePagesManager>();
      manager.RegisterServices(services, webHostEnvironment);
   }
}

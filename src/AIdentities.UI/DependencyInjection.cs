using AIdentities.Shared.Plugins.Storage;
using AIdentities.UI.Features.AIdentityManagement.Services;
using AIdentities.UI.Features.Core.Services.PageManager;
using AIdentities.UI.Features.Core.Services.Plugins;
using AIdentities.UI.Features.Core.Services.PluginStaticResources;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace AIdentities.UI;

public static class DependencyInjection
{
   public static IWebHostBuilder UseStaticPluginWebAssets(this IWebHostBuilder builder, IFileProvider provider)
   {
      builder.ConfigureAppConfiguration((context, configBuilder) =>
      {
         context.HostingEnvironment.WebRootFileProvider = new CompositeFileProvider(new[] { provider, context.HostingEnvironment.WebRootFileProvider });
      });
      return builder;
   }


   public static IServiceCollection AddAIdentitiesServices(
      this IServiceCollection services,
      IWebHostEnvironment webHostEnvironment,
      out ILogger startupLogger,
      out IFileProvider pluginStaticWebProvider)
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

      // Note: AppOptions will be validated explicitly during plugin loading.
      // Plugin developers who wants to use IOptions validator should instead register
      // validators and use ValidateOnStart.
      services
         .AddOptions<AppOptions>()
         .BindConfiguration(AppOptions.SECTION_NAME);

      services.AddMudServices();
      services.AddBlazoredLocalStorage();

      services.AddScoped<IPageDefinitionProvider, PageDefinitionProvider>();
      services.AddScoped<INotificationService, NotificationService>();
      services.AddScoped<IAppComponentSettingsManager, AppComponentSettingsManager>();

      services
         .AddScoped<EventAggregator.Blazor.IEventAggregator, EventAggregator.Blazor.EventAggregator>()
         .AddScoped<IEventBus, EventBus>();

      services
         .AddSingleton<IAIdentityProvider, AIdentityProvider>()
         .AddSingleton<AIdentityProviderSerializationSettings>();

      //services.AddSingleton<>

      services.AddScoped<IScrollService, ScrollService>();

      startupLogger = RegisterPlugins(services, webHostEnvironment, out pluginStaticWebProvider);

      return services;
   }

   private static ILogger RegisterPlugins(IServiceCollection services, IWebHostEnvironment webHostEnvironment, out IFileProvider pluginStaticWebProvider)
   {
      services.AddSingleton<IPluginStorageFactory, PluginStorageFactory>();
      services.AddSingleton<IPluginFoldersProvider, PluginFoldersProvider>();
      services.AddSingleton<IPackageInspector, PackageInspector>();

      // this is used by debuggable modules to register their services.
      // since we need to build a temporary service provider to deal with plugins, we register this service now to not have to build another one later.
      services.AddSingleton<IDebuggablePagesManager, DebuggablePagesManager>();


      // in order to let plugins register their services, we need to load packages before we can build the service provider.
      // at the moment to not duplicate code, we create a new instance of the plugin manager here in order to load the plugin assemblies.
      // before creating a new temporary service provider, we create a temporary service collection so we can add some registration only needed
      // for the plugin loading.
      IServiceCollection temporaryServices = new ServiceCollection();
      foreach (var service in services)
      {
         temporaryServices.Add(service);
      }

      // we add the PluginManager to the temporary service collection so we can load plugins.
      temporaryServices.AddSingleton<PluginManager>();
      // we add the PluginStaticResourceProvider to the temporary service collection so we can load plugins.
      temporaryServices.AddSingleton<PluginStaticResourceProvider>();
      // we add the AppOptionsValidator to the temporary service collection so we can validate the AppOptions explicitly before registering plugins.
      temporaryServices.AddSingleton<AppOptionsValidator>();
      // Register debuggable module services.
      RegisterDebuggableModules(temporaryServices);

      // we build a temporary service provider so we can use it to load plugins.
      var temporaryServiceProvider = temporaryServices.BuildServiceProvider();
      var startupLogger = temporaryServiceProvider.GetRequiredService<ILogger<Program>>();

      // before registering plugins, we ensure AppOptions are validated explicitly because
      // we are going to use services that uses the Options and since the ValidateOnStart is not invoked yet,
      // we are causing validation to happen during DI resolution and we won't have control on the output
      ValidateAppOptionsExplicitly(startupLogger, temporaryServiceProvider);

      PluginManager pluginManager = temporaryServiceProvider.GetRequiredService<PluginManager>();
      pluginManager.LoadStoredPackagesAsync(services).GetAwaiter().GetResult();

      // TODO: this should be improved, by splitting the plugin manager in two parts:
      // - one that loads packages at startup from disk and allow to get the list of loaded packages and their status
      // - one that take the result of the first and manage their lifecycle
      services
         .AddSingleton<IPluginManager, PluginManager>(sp =>
         {
            pluginManager.SwapDependencies(
               logger: sp.GetRequiredService<ILogger<PluginManager>>(),
               options: sp.GetRequiredService<IOptions<AppOptions>>(),
               pluginFoldersProvider: sp.GetRequiredService<IPluginFoldersProvider>(),
               pluginStorageFactory: sp.GetRequiredService<IPluginStorageFactory>(),
               packageInspector: sp.GetRequiredService<IPackageInspector>()
               );

            return pluginManager;
         });

      //same for the static resource provider
      PluginStaticResourceProvider pluginStaticResourceProvider = temporaryServiceProvider.GetRequiredService<PluginStaticResourceProvider>();
      pluginStaticResourceProvider.Initialize(pluginManager.LoadedPackages);
      services
         .AddSingleton<IPluginStaticResourceProvider, PluginStaticResourceProvider>(sp =>
         {
            pluginStaticResourceProvider.InjectDependencies(
               logger: sp.GetRequiredService<ILogger<IPluginStaticResourceProvider>>(),
               pluginFoldersProvider: sp.GetRequiredService<IPluginFoldersProvider>()
               );

            return pluginStaticResourceProvider;
         });


      // We are using an instance of IDebuggablePagesManager by using the temporary service provider we created for plugin loading.
      // The implementation instantiate the the registered IDebuggableModule implementations registered so far (plugin developers
      // should have done so in RegisterDebuggableModules method) and then invoke the method RegisterServices on resolved found modules
      // passesing the original services collection and the web host environment.
      IDebuggablePagesManager debuggablePagesManager = temporaryServiceProvider.GetRequiredService<IDebuggablePagesManager>();
      debuggablePagesManager.RegisterServices(services, webHostEnvironment);


      // we return the pluginStaticResourceProvider so we can use it to register static resources.
      pluginStaticWebProvider = pluginStaticResourceProvider;

      return startupLogger;
   }

   /// <summary>
   /// We instantiate a logger and the AppOptions to validate them explicitly and give a nice output in case of validation errors.
   /// </summary>
   /// <param name="temporaryServiceProvider"></param>
   private static void ValidateAppOptionsExplicitly(ILogger logger, ServiceProvider temporaryServiceProvider)
   {
      logger.LogInformation("Validating AppOptions...");
      var appOptionsValidator = temporaryServiceProvider.GetRequiredService<AppOptionsValidator>();
      var appOptions = temporaryServiceProvider.GetRequiredService<IOptions<AppOptions>>();
      var result = appOptionsValidator.Validate(null, appOptions.Value);

      if (result.Succeeded)
      {
         logger.LogInformation("AppOptions validation succeeded.");
      }
      else
      {
         logger.LogError("AppOptions validation failed.");
         if (result.Failures != null)
         {
            foreach (var failure in result.Failures)
            {
               logger.LogError($"- {failure}");
            }
         }
         throw new Exception("AppOptions validation failed.");
      }
   }

   /// <summary>
   /// In order to let debuggable modules register their services, we need to register them.
   /// This is a manual process, since we cannot use the plugin manager to do this for us.
   /// So everytime we want to develop a plugin but we want to take advantage of the debuggable modules in a more
   /// comfortable way, we need to register them here.
   /// </summary>
   /// <param name="services">The service collection where to register plugin services.</param>
   private static void RegisterDebuggableModules(IServiceCollection services)
   {
      services.AddSingleton<IDebuggableModule, BooruAIdentityImporter.DebuggableModule>();
   }
}

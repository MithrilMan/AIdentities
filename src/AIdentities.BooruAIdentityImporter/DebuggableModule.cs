namespace AIdentities.BooruAIdentityImporter;

public sealed class DebuggableModule : IDebuggableModule
{
   public void RegisterServices(IServiceCollection services, IHostEnvironment hostEnvironment)
   {
      var plugin = new BooruAIdentityImporterPlugin();

      var manifest = new PluginManifest()
      {
         Signature = new()
         {
            Name = "BooruAIdentityImporter",
            Version = "0.0.1-DEBUG",
            Author = "Mithril Man"
         },
         Description = "AIdentities plugin for importing AIdentities from boorus.",
      };

      var sp = services.BuildServiceProvider();
      var pluginStorageFactory = sp.GetRequiredService<IPluginStorageFactory>();
      var pluginStorage = pluginStorageFactory.CreatePluginStorage(manifest);

      plugin.Initialize(manifest, services, pluginStorage);

      // important, to expose this assembly when referencing this plugin directly in the AIdentities project.
      services.AddSingleton<IDebuggableModule, DebuggableModule>();
   }
}

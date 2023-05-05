namespace AIdentities.BooruAIdentityImporter;

public class PluginEntry : IPluginEntry
{
   private PluginManifest _manifest = default!;

   public void Initialize(PluginManifest manifest, IServiceCollection services, IPluginStorage pluginStorage)
   {
      _manifest = manifest;

      RegisterServices(services);
   }

   public void RegisterServices(IServiceCollection services)
   {
      services.AddScoped<IAIdentityImporter, Services.BooruAIdentityImporter>();
   }
}

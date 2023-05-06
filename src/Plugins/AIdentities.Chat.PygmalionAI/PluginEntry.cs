
namespace AIdentities.Chat.PygmalionAI;

public class PluginEntry : IPluginEntry
{
   private PluginManifest _manifest = default!;
   private IPluginStorage _storage = default!;

   public void Initialize(PluginManifest manifest, IServiceCollection services, IPluginStorage pluginStorage)
   {
      _manifest = manifest;
      _storage = pluginStorage;

      RegisterServices(services);
   }

   public void RegisterServices(IServiceCollection services)
   {

   }
}

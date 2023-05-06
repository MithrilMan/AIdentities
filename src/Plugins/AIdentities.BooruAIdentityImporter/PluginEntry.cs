using AIdentities.BooruAIdentityImporter.Services.FormatDetector;
using AIdentities.BooruAIdentityImporter.Services.FormatDetector.Formats;

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
      services.AddSingleton<IFormatDetector, FormatDetector>();

      RegisterFormatDetectors(services);
   }


   private static void RegisterFormatDetectors(IServiceCollection services) => services
            .AddSingleton<ICharacterFormatDetector, CaiCharacterFormat>()
            .AddSingleton<ICharacterFormatDetector, CaiHistoryFormat>()
            .AddSingleton<ICharacterFormatDetector, TavernCharacterFormat>()
            .AddSingleton<ICharacterFormatDetector, TextGenerationCharacterFormat>()
            ;
}

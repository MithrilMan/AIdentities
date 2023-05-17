using AIdentities.BooruAIdentityImporter.Services.FormatDetector;
using AIdentities.BooruAIdentityImporter.Services.FormatDetector.Formats;

namespace AIdentities.BooruAIdentityImporter;

public class PluginEntry : BasePluginEntry<PluginEntry>
{
   public override void RegisterServices(IServiceCollection services)
   {
      services.AddScoped<IAIdentityImporter, Services.BooruAIdentityImporter>();
      services.AddSingleton<IFormatDetector, FormatDetector>();

      RegisterFormatDetectors(services);

      RegisterAIdentitySafetyChecker<BooruAIdentitySafetyChecker>();
   }

   private static void RegisterFormatDetectors(IServiceCollection services) => services
         .AddSingleton<ICharacterFormatDetector, CaiCharacterFormat>()
         .AddSingleton<ICharacterFormatDetector, CaiHistoryFormat>()
         .AddSingleton<ICharacterFormatDetector, TavernCharacterFormat>()
         .AddSingleton<ICharacterFormatDetector, TextGenerationCharacterFormat>()
         ;
}

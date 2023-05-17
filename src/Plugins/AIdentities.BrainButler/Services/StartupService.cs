using AIdentities.Shared.Features.Core.Services;

namespace AIdentities.BrainButler.Services;
public class StartupService : IPluginStartup
{
   readonly ThemeUpdater _themeUpdater;

   public StartupService(ThemeUpdater themeUpdater)
   {
      _themeUpdater = themeUpdater;
   }

   public async ValueTask StartupAsync()
   {
      await _themeUpdater.LoadPaletteFromDisk().ConfigureAwait(false);
   }
}

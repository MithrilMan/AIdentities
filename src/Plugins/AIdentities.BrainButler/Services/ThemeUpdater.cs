using AIdentities.BrainButler.Commands.ChangeTheme;
using AIdentities.Shared.Features.Core.Services;

namespace AIdentities.BrainButler.Services;

/// <summary>
/// Questo servizio parte all'avvio dell'applicazione (IHostedService) e si occupa di gestire il tema dell'applicazione.
/// </summary>
public class ThemeUpdater
{
   public const string LIGHT_STORED_THEME_FILE_NAME = "generated_theme_light.json";
   public const string LIGHT_STORED_THEME_FILE_NAME_BACKUP = "generated_theme_light.json.bak";
   public const string DARK_STORED_THEME_FILE_NAME = "generated_theme_dark.json";
   public const string DARK_STORED_THEME_FILE_NAME_BACKUP = "generated_theme_dark.json.bak";

   readonly ILogger<ThemeUpdater> _logger;
   readonly IThemeManager _themeUpdater;
   readonly IPluginStorage<PluginEntry> _pluginStorage;

   public bool IsDarkMode => _themeUpdater.IsDarkMode;

   public ThemeUpdater(ILogger<ThemeUpdater> logger, IThemeManager themeManager, IPluginStorage<PluginEntry> pluginStorage)
   {
      _logger = logger;
      _themeUpdater = themeManager;
      _pluginStorage = pluginStorage;
   }

   public MudTheme GetTheme() => _themeUpdater.GetTheme();
   public void SetTheme(MudTheme theme) => _themeUpdater.SetTheme(theme);


   public async ValueTask SavePaletteToDisk(string proposedPalette, bool? isDarkMode)
   {
      if (isDarkMode ?? _themeUpdater.IsDarkMode)
      {
         await _pluginStorage.BackupFileAsync(DARK_STORED_THEME_FILE_NAME, DARK_STORED_THEME_FILE_NAME_BACKUP).ConfigureAwait(false);
         await _pluginStorage.WriteAsync(DARK_STORED_THEME_FILE_NAME, proposedPalette).ConfigureAwait(false);
      }
      else
      {
         await _pluginStorage.BackupFileAsync(LIGHT_STORED_THEME_FILE_NAME, LIGHT_STORED_THEME_FILE_NAME_BACKUP).ConfigureAwait(false);
         await _pluginStorage.WriteAsync(LIGHT_STORED_THEME_FILE_NAME, proposedPalette).ConfigureAwait(false);
      }
   }

   public async ValueTask LoadPaletteFromDisk()
   {
      var theme = _themeUpdater.GetTheme()!;

      var lightPalette = await _pluginStorage.ReadAsync(LIGHT_STORED_THEME_FILE_NAME).ConfigureAwait(false);
      if (lightPalette != null)
      {
         var lightPaletteDto = JsonSerializer.Deserialize<PaletteReference>(lightPalette, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
         if (lightPaletteDto != null)
         {
            theme.Palette = PaletteMapper.CreateLightPalette(lightPaletteDto);
         }
      }

      var darkPalette = await _pluginStorage.ReadAsync(DARK_STORED_THEME_FILE_NAME).ConfigureAwait(false);
      if (darkPalette != null)
      {
         var darkPaletteDto = JsonSerializer.Deserialize<PaletteReference>(darkPalette, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
         if (darkPaletteDto != null)
         {
            theme.Palette = PaletteMapper.CreateDarkPalette(darkPaletteDto);
         }
      }

      _themeUpdater.SetTheme(theme);
   }
}

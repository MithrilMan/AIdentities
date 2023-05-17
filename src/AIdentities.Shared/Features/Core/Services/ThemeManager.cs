using MudBlazor;
using MudBlazor.Utilities;

namespace AIdentities.Shared.Features.Core.Services;

public class ThemeManager : IThemeManager
{
   readonly ILogger<ThemeManager> _logger;
   private MudTheme _theme;

   public event EventHandler? ThemeChanged;
   public bool IsDarkMode { get; set; }

   public ThemeManager(ILogger<ThemeManager> logger)
   {
      _logger = logger;
      _theme = CreateTheme();
   }

   public MudTheme GetTheme() => _theme;

   public void SetTheme(MudTheme theme)
   {
      _theme = theme;
      ThemeChanged?.Invoke(this, EventArgs.Empty);
   }

   MudTheme CreateTheme() => new MudTheme()
   {
      Palette = new PaletteLight
      {
         Primary = FromRgb(27, 38, 44),
         Secondary = FromRgb(15, 76, 117),
         Tertiary = FromRgb(50, 130, 184),
         Background = FromRgb(245, 245, 245),
         Surface = FromRgb(255, 255, 255),
         DrawerBackground = FromRgb(235, 235, 235),
         DrawerText = FromRgb(27, 38, 44),
         AppbarBackground = FromRgb(255, 255, 255),
         AppbarText = FromRgb(27, 38, 44),
         TextPrimary = FromRgb(27, 38, 44),
         TextSecondary = FromRgb(15, 76, 117),
         TextDisabled = FromRgb(175, 175, 175),
         ActionDefault = FromRgb(27, 38, 44),
         ActionDisabled = FromRgb(175, 175, 175),
         Divider = FromRgb(215, 215, 215),
         DividerLight = FromRgb(235, 235, 235),
         TableLines = FromRgb(235, 235, 235),
      },
      PaletteDark = new PaletteDark
      {
         Primary = "#0084b4",
         Secondary = "#1ca1f2",
         Tertiary = "#a6d7f6",
         Background = "#272d30",
         Surface = "#2d3239",
         DrawerBackground = "#353a3f",
         DrawerText = "#ffffff",
         AppbarBackground = "#272d30",
         AppbarText = "#ffffff",
         TextPrimary = "#ffffff",
         TextSecondary = "#bab3b3",
         TextDisabled = "#a1a1a1",
         ActionDefault = "#0084b4",
         ActionDisabled = "#a1a1a1",
         Divider = "#2a363f",
         DividerLight = "#353a3f",
         TableLines = "#353a3f",
      }
   };

   public static MudColor FromRgb(byte red, int green, int blue)
   {
      return new MudColor(red, green, blue, 0xFF);
   }
}

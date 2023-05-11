using Microsoft.AspNetCore.Components;
using MudBlazor.Utilities;

namespace AIdentities.UI.Features.Core.Pages;

public partial class MainLayout
{
   [Inject] private ILocalStorageService LocalStorageService { get; set; } = default!;

   private MudThemeProvider _mudThemeProvider = default!;
   private MudTheme _theme = default!;

   protected override void OnInitialized()
   {
      base.OnInitialized();
      _theme = CreateTheme();
      // _theme = new MudTheme();
   }

   protected override async Task OnAfterRenderAsync(bool firstRender)
   {
      if (firstRender)
      {
         var storedTheme = await LocalStorageService.GetItemAsync<bool?>(AppConstants.LocalStorage.THEME).ConfigureAwait(false);
         _state.IsDarkMode = storedTheme ?? await _mudThemeProvider.GetSystemPreference().ConfigureAwait(false);
         await InvokeAsync(StateHasChanged).ConfigureAwait(false);
      }
   }

   void DrawerToggle() => _state.IsDrawerOpen = !_state.IsDrawerOpen;

   private async Task SaveTheme() => await LocalStorageService.SetItemAsync(AppConstants.LocalStorage.THEME, _state.IsDarkMode).ConfigureAwait(false);

   MudTheme CreateTheme()
   {
      return new MudTheme()
      {
         Palette = LightPalettes.Palettes.Last(),
         PaletteDark = DarkPalettes.Palettes.SkipLast(0).Last()
      };
   }


   public class Palettes
   {
      public static MudColor FromRgb(byte red, int green, int blue)
      {
         return new MudColor(red, green, blue, 0xFF);
      }
   }

   internal class LightPalettes : Palettes
   {
      public static List<PaletteLight> Palettes { get; } = new()
      {
         new PaletteLight
         {
             Primary = FromRgb(255, 152, 0), // Light orange
             Secondary = FromRgb(0, 121, 107), // Dark teal
             Tertiary = FromRgb(255, 110, 0), // Bright orange
             Background = FromRgb(249, 249, 249), // Light gray
             Surface = FromRgb(255, 255, 255), // White
             DrawerBackground = FromRgb(242, 242, 242), // Lighter gray
             DrawerText = FromRgb(51, 51, 51), // Dark gray
             AppbarBackground = FromRgb(255, 255, 255), // White
             AppbarText = FromRgb(51, 51, 51), // Dark gray
             TextPrimary = FromRgb(51, 51, 51), // Dark gray
             TextSecondary = FromRgb(153, 153, 153), // Light gray
             TextDisabled = FromRgb(204, 204, 204), // Lighter gray
             ActionDefault = FromRgb(51, 51, 51), // Dark gray
             ActionDisabled = FromRgb(204, 204, 204), // Lighter gray
             Divider = FromRgb(224, 224, 224), // Light gray
             DividerLight = FromRgb(240, 240, 240), // Lighter gray
             TableLines = FromRgb(240, 240, 240), // Lighter gray
             Info = FromRgb(0, 153, 204), // Bright blue
             Success = FromRgb(0, 204, 102), // Bright green
             Warning = FromRgb(255, 193, 7), // Yellow
             Error = FromRgb(220, 53, 69) // Red
         },
         new PaletteLight {
            Primary = FromRgb(31, 40, 51),
            Secondary = FromRgb(75, 101, 135),
            Tertiary = FromRgb(102, 225, 211),
            Background = FromRgb(245, 245, 245),
            Surface = FromRgb(255, 255, 255),
            DrawerBackground = FromRgb(235, 235, 235),
            DrawerText = FromRgb(31, 40, 51),
            AppbarBackground = FromRgb(255, 255, 255),
            AppbarText = FromRgb(31, 40, 51),
            TextPrimary = FromRgb(31, 40, 51),
            TextSecondary = FromRgb(75, 101, 135),
            TextDisabled = FromRgb(175, 175, 175),
            ActionDefault = FromRgb(31, 40, 51),
            ActionDisabled = FromRgb(175, 175, 175),
            Divider = FromRgb(215, 215, 215),
            DividerLight = FromRgb(235, 235, 235),
            TableLines = FromRgb(235, 235, 235),
            Info = FromRgb(102, 225, 211),
            Success = FromRgb(0, 204, 102),
            Warning = FromRgb(255, 193, 7),
            Error = FromRgb(220, 53, 69)
         },
         new PaletteLight {
            Primary = FromRgb(36, 32, 56),
            Secondary = FromRgb(114, 88, 115),
            Tertiary = FromRgb(214, 162, 232),
            Background = FromRgb(245, 245, 245),
            Surface = FromRgb(255, 255, 255),
            DrawerBackground = FromRgb(235, 235, 235),
            DrawerText = FromRgb(36, 32, 56),
            AppbarBackground = FromRgb(255, 255, 255),
            AppbarText = FromRgb(36, 32, 56),
            TextPrimary = FromRgb(36, 32, 56),
            TextSecondary = FromRgb(114, 88, 115),
            TextDisabled = FromRgb(175, 175, 175),
            ActionDefault = FromRgb(36, 32, 56),
            ActionDisabled = FromRgb(175, 175, 175),
            Divider = FromRgb(215, 215, 215),
            DividerLight = FromRgb(235, 235, 235),
            TableLines = FromRgb(235, 235, 235),
            Info = FromRgb(214, 162, 232),
            Success = FromRgb(0, 204, 102),
            Warning = FromRgb(255, 193, 7),
            Error = FromRgb(220, 53, 69)
         },
         new PaletteLight {
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
            Info = FromRgb(50, 130, 184),
            Success = FromRgb(0, 204, 102),
            Warning = FromRgb(255, 193, 7),
            Error = FromRgb(220, 53, 69)
         }
      };
   }

   internal class DarkPalettes : Palettes
   {
      public static List<PaletteDark> Palettes { get; } = new() {
         new PaletteDark
         {
             Primary = FromRgb(255, 152, 0), // Light orange
             Secondary = FromRgb(0, 121, 107), // Dark teal
             Tertiary = FromRgb(255, 110, 0), // Bright orange
             Background = FromRgb(66, 66, 66), // Dark gray
             Surface = FromRgb(51, 51, 51), // Medium gray
             DrawerBackground = FromRgb(61, 61, 61), // Lighter medium gray
             DrawerText = FromRgb(255, 255, 255), // White
             AppbarBackground = FromRgb(51, 51, 51), // Medium gray
             AppbarText = FromRgb(255, 255, 255), // White
             TextPrimary = FromRgb(255, 255, 255), // White
             TextSecondary = FromRgb(179, 179, 179), // Light gray
             TextDisabled = FromRgb(102, 102, 102), // Dark gray
             ActionDefault = FromRgb(255, 255, 255), // White
             ActionDisabled = FromRgb(179, 179, 179), // Light gray
             Divider = FromRgb(77, 77, 77), // Medium dark gray
             DividerLight = FromRgb(128, 128, 128), // Light gray
             TableLines = FromRgb(64, 64, 64), // Medium dark gray
             Info = FromRgb(0, 153, 204), // Bright blue
             Success = FromRgb(0, 204, 102), // Bright green
             Warning = FromRgb(255, 193, 7), // Yellow
             Error = FromRgb(220, 53, 69) // Red
         },
         new PaletteDark {
            Primary = FromRgb(31, 40, 51),
            Secondary = FromRgb(75, 101, 135),
            Tertiary = FromRgb(102, 225, 211),
            Background = FromRgb(21, 27, 35),
            Surface = FromRgb(26, 34, 45),
            DrawerBackground = FromRgb(18, 24, 31),
            DrawerText = FromRgb(202, 225, 235),
            AppbarBackground = FromRgb(26, 34, 45),
            AppbarText = FromRgb(202, 225, 235),
            TextPrimary = FromRgb(202, 225, 235),
            TextSecondary = FromRgb(150, 180, 190),
            TextDisabled = FromRgb(100, 130, 140),
            ActionDefault = FromRgb(202, 225, 235),
            ActionDisabled = FromRgb(100, 130, 140),
            Divider = FromRgb(50, 65, 85),
            DividerLight = FromRgb(75, 95, 120),
            TableLines = FromRgb(75, 95, 120),
            Info = FromRgb(102, 225, 211),
            Success = FromRgb(0, 204, 102),
            Warning = FromRgb(255, 193, 7),
            Error = FromRgb(220, 53, 69)
         },
         new PaletteDark {
            Primary = FromRgb(36, 32, 56),
            Secondary = FromRgb(114, 88, 115),
            Tertiary = FromRgb(214, 162, 232),
            Background = FromRgb(28, 24, 43),
            Surface = FromRgb(31, 27, 48),
            DrawerBackground = FromRgb(24, 20, 36),
            DrawerText = FromRgb(235, 205, 245),
            AppbarBackground = FromRgb(31, 27, 48),
            AppbarText = FromRgb(235, 205, 245),
            TextPrimary = FromRgb(235, 205, 245),
            TextSecondary = FromRgb(190, 160, 205),
            TextDisabled = FromRgb(140, 110, 155),
            ActionDefault = FromRgb(235, 205, 245),
            ActionDisabled = FromRgb(140, 110, 155),
            Divider = FromRgb(85, 60, 100),
            DividerLight = FromRgb(120, 85, 135),
            TableLines = FromRgb(120, 85, 135),
            Info = FromRgb(214, 162, 232),
            Success = FromRgb(0, 204, 102),
            Warning = FromRgb(255, 193, 7),
            Error = FromRgb(220, 53, 69)
         },
         new PaletteDark {
            Primary = FromRgb(27, 38, 44),
            Secondary = FromRgb(15, 76, 117),
            Tertiary = FromRgb(50, 130, 184),
            Background = FromRgb(18, 26, 31),
            Surface = FromRgb(22, 33, 39),
            DrawerBackground = FromRgb(14, 20, 24),
            DrawerText = FromRgb(200, 240, 255),
            AppbarBackground = FromRgb(22, 33, 39),
            AppbarText = FromRgb(200, 240, 255),
            TextPrimary = FromRgb(200, 240, 255),
            TextSecondary = FromRgb(150, 190, 205),
            TextDisabled = FromRgb(100, 140, 155),
            ActionDefault = FromRgb(200, 240, 255),
            ActionDisabled = FromRgb(100, 140, 155),
            Divider = FromRgb(35, 50, 60),
            DividerLight = FromRgb(65, 90, 110),
            TableLines = FromRgb(65, 90, 110),
            Info = FromRgb(50, 130, 184),
            Success = FromRgb(0, 204, 102),
            Warning = FromRgb(255, 193, 7),
            Error = FromRgb(220, 53, 69)
         }
      };
   }
}

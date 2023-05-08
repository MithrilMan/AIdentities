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
      var primaryHue = 35; // yellow-green
      var secondaryHue = 250; // blue-purple
      var tertiaryHue = (primaryHue + secondaryHue) / 2; // green-blue


      return new MudTheme()
      {
         Palette = LightPalettes.Palettes.Last(),
         PaletteDark = DarkPalettes.Palettes.Last()
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
         }
      };
   }
}

using Microsoft.AspNetCore.Components;

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
      _theme = new MudTheme();
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

   MudTheme CreateTheme() => new MudTheme()
   {
      Palette = new PaletteLight()
      {
         Primary = "#546d78",
         Secondary = "#546d78",
         Tertiary = "#546d78",
         //AppbarBackground = "#546d78",
      },
      PaletteDark = new PaletteDark()
      {
         Primary = "#2d3749",
         Secondary = "#dd6c19",
         Tertiary = "#546d78",
         //AppbarBackground = "#1a212c"
      }
   };
}

using Microsoft.AspNetCore.Components;

namespace AIdentities.UI.Features.Core.Pages;

public partial class MainLayout
{
   [Inject] private ILocalStorageService LocalStorageService { get; set; } = default!;
   [Inject] private IThemeManager ThemeManager { get; set; } = default!;

   private MudThemeProvider _mudThemeProvider = default!;

   protected override void OnInitialized()
   {
      base.OnInitialized();
      ThemeManager.ThemeChanged += OnThemeChanged;
   }

   private async void OnThemeChanged(object? sender, EventArgs e)
   {
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);
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

   void OnDarkModeChanged()
   {
      ThemeManager.IsDarkMode = _state.IsDarkMode;
   }
}

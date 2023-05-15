using MudBlazor;

namespace AIdentities.Shared.Features.Core.Services;

public class ThemeManager : IThemeManager
{
   readonly ILogger<ThemeManager> _logger;
   private MudTheme? _theme;
   public event EventHandler? ThemeChanged;

   public ThemeManager(ILogger<ThemeManager> logger)
   {
      _logger = logger;
   }

   public MudTheme? GetTheme() => _theme;

   public void SetTheme(MudTheme theme)
   {
      _theme = theme;
      ThemeChanged?.Invoke(this, EventArgs.Empty);
   }
}

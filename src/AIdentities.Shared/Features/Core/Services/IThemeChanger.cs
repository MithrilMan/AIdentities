using MudBlazor;

namespace AIdentities.Shared.Features.Core.Services;

/// <summary>
/// Allows to change the theme at runtime.
/// </summary>
public interface IThemeManager
{
   /// <summary>
   /// Occurs when the theme has changed.
   /// </summary>
   event EventHandler ThemeChanged;

   /// <summary>
   /// Sets a specific theme.
   /// </summary>
   /// <param name="theme">The theme to set.</param>
   void SetTheme(MudTheme theme);

   /// <summary>
   /// Gets the current theme.
   /// </summary>
   /// <returns>The current theme.</returns>
   MudTheme? GetTheme();
}

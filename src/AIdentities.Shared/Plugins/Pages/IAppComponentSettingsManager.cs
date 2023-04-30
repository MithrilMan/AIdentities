namespace AIdentities.Shared.Plugins.Pages;
public interface IAppComponentSettingsManager
{
   TAppComponentSettings? GetSettings<TAppComponentSettings>(string settingsKey)
       where TAppComponentSettings : class, IAppComponentSettings, new();

   bool SetSettings<TAppComponentSettings>(string settingsKey, TAppComponentSettings settings)
      where TAppComponentSettings : class, IAppComponentSettings, new();

   ValueTask<bool> SaveSettingsAsync(CancellationToken cancellationToken);
}

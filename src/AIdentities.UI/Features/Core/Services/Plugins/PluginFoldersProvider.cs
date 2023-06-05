using Microsoft.Extensions.Options;

namespace AIdentities.UI.Features.Core.Services.Plugins;

public class PluginFoldersProvider : IPluginFoldersProvider
{
   const string ENABLED_PLUGINS_FILE_NAME = "enabled-plugins.json";

   readonly ILogger<PluginFoldersProvider> _logger;
   readonly IOptions<AppOptions> _options;
   readonly IHostEnvironment _hostEnvironment;

   readonly string _pluginAssetsPath;
   readonly string _pluginsStoragePath;

   public PluginFoldersProvider(ILogger<PluginFoldersProvider> logger, IOptions<AppOptions> options, IHostEnvironment hostEnvironment)
   {
      _logger = logger;
      _options = options;
      _hostEnvironment = hostEnvironment;

      var packageFolderRoot = PathUtils.GetAbsolutePath(_options.Value.PackageFolder, _hostEnvironment.ContentRootPath);
      logger.LogDebug("Plugin package folder root: {PackageFolderRoot}", packageFolderRoot);
      if(packageFolderRoot.Contains("bin")) throw new Exception("Package folder root cannot be in bin folder");

      _pluginAssetsPath = Path.Combine(packageFolderRoot, AppConstants.SpecialFolders.PLUGINS);
      logger.LogDebug("Checking folder: {PackageFolderRoot}", _pluginAssetsPath);
      if (!Directory.Exists(_pluginAssetsPath))
      {
         Directory.CreateDirectory(_pluginAssetsPath);
      }

      _pluginsStoragePath = Path.Combine(packageFolderRoot, AppConstants.SpecialFolders.STORAGE);
      logger.LogDebug("Checking folder: {PackageFolderRoot}", _pluginsStoragePath);
      if (!Directory.Exists(_pluginsStoragePath))
      {
         Directory.CreateDirectory(_pluginsStoragePath);
      }
   }

   public string GetPluginAssetsPath() => _pluginAssetsPath;

   public string GetPluginsStoragePath() => _pluginsStoragePath;

   public string GetPluginInstallationRoot(PluginManifest pluginManifest)
      => Path.Combine(_pluginAssetsPath, pluginManifest.Signature.GetFullName());

   public string GetPluginInstallationRoot(string pluginFullName)
      => Path.Combine(_pluginAssetsPath, pluginFullName);

   public string GetPluginEnabledFilePath() => Path.Combine(_pluginAssetsPath, ENABLED_PLUGINS_FILE_NAME);
}

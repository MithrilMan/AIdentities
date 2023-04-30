using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AIdentities.Shared.Plugins.Storage;

public class PluginStorageFactory : IPluginStorageFactory
{
   readonly ILoggerFactory _loggerFactory;
   readonly IOptions<AppOptions> _options;
   readonly IHostEnvironment _hostEnvironment;

   readonly ILogger<PluginStorageFactory> _logger;
   readonly string _pluginStorageRoot;

   public PluginStorageFactory(ILoggerFactory loggerFactory, IOptions<AppOptions> options, IHostEnvironment hostEnvironment)
   {
      _loggerFactory = loggerFactory;
      _options = options;
      _hostEnvironment = hostEnvironment;

      _logger = loggerFactory.CreateLogger<PluginStorageFactory>();
      _pluginStorageRoot = GetPluginStoragePath();

      if (!Directory.Exists(_pluginStorageRoot))
      {
         Directory.CreateDirectory(_pluginStorageRoot);
      }
   }

   private string GetPluginStoragePath()
      => Path.Combine(PathUtils.GetAbsolutePath(_options.Value.PackageFolder, _hostEnvironment.ContentRootPath), AppConstants.SpecialFolders.STORAGE);

   public PluginStorage CreatePluginStorage(PluginManifest pluginManifest)
   {
      return new PluginStorage(_loggerFactory.CreateLogger<PluginStorage>(), pluginManifest, _pluginStorageRoot);
   }
}

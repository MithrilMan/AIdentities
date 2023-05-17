using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using AIdentities.Shared.Plugins.Storage;
using McMaster.NETCore.Plugins;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;

namespace AIdentities.UI.Features.Core.Services.Plugins;

public class PluginManager : IPluginManager
{
   const string MANIFEST_FILE_NAME = "content/aid-manifest.json";

   ILogger<PluginManager> _logger;
   IPluginFoldersProvider _pluginFoldersProvider;
   AppOptions _options;
   IPluginStorageFactory _pluginStorageFactory;
   IPackageInspector _packageInspector;
   readonly string[] _whitelistedResourceFiles = new string[] {
      "Microsoft.AspNetCore.StaticWebAssets.props"
   };


   public IEnumerable<Package> LoadedPackages => _loadedPackages;
   public IEnumerable<PluginStatus> StoredPlugins => _storedPlugins;
   public IReadOnlyDictionary<string, string> InvalidPlugins => _invalidPlugins;
   public IEnumerable<Assembly> PagePluginAssemblies => _pluginPageAssemblies.Values;

   readonly List<Package> _loadedPackages = new();
   readonly List<PluginStatus> _storedPlugins = new();
   readonly Dictionary<string, string> _invalidPlugins = new();
   readonly Dictionary<string, Assembly> _pluginPageAssemblies = new();

   readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
   private bool _storeLoaded = false;

   // define an event PackageLoaded
   public event EventHandler<Package>? PackageLoaded;

   public PluginManager(ILogger<PluginManager> logger,
                        IOptions<AppOptions> options,
                        IPluginFoldersProvider pluginFoldersProvider,
                        IPluginStorageFactory pluginStorageFactory,
                        IPackageInspector packageInspector)
   {
      _logger = logger;
      _pluginFoldersProvider = pluginFoldersProvider;
      _options = options.Value;
      _pluginStorageFactory = pluginStorageFactory;
      _packageInspector = packageInspector;
   }

   /// <summary>
   /// This method is used to swap the dependencies of the PluginManager.
   /// In order to let plugins inject their services into the DI container, the PluginManager
   /// must be instantiated before the DI container is built.
   /// Once the plugins are loaded in memory we have to hold the instance of the PluginManager
   /// because of it's internal state, but we need to swap the dependencies of the PluginManager
   /// with the ones that belongs to the final build DI container.
   /// </summary>
   /// <param name="logger">The logger.</param>
   /// <param name="options">The app options.</param>
   internal void SwapDependencies(ILogger<PluginManager> logger,
                                  IOptions<AppOptions> options,
                                  IPluginFoldersProvider pluginFoldersProvider,
                                  IPluginStorageFactory pluginStorageFactory,
                                  IPackageInspector packageInspector)
   {
      _logger = logger;
      _options = options.Value;
      _pluginFoldersProvider = pluginFoldersProvider;
      _pluginStorageFactory = pluginStorageFactory;
      _packageInspector = packageInspector;
   }

   public async ValueTask<PluginStatus> StorePackageAsync(IBrowserFile packageFile)
   {
      using MemoryStream ms = new();
      await packageFile.OpenReadStream().CopyToAsync(ms).ConfigureAwait(false);
      var bytes = ms.ToArray();

      using var archive = new ZipArchive(new MemoryStream(bytes));

      PluginManifest manifest = ReadManifest(archive);

      if (_storedPlugins.Any(p => p.Manifest.Signature == manifest.Signature))
         throw new InvalidPluginException($"Plugin {manifest.Signature.GetFullName()} is already installed.");

      string packageRootFolder = _pluginFoldersProvider.GetPluginInstallationRoot(manifest);
      Directory.CreateDirectory(packageRootFolder);

      bool hasResourceExtensionsFilter = _options.AllowedPluginResourceExtensions.Any();
      // Extract only allowed files
      foreach (ZipArchiveEntry entry in archive.Entries)
      {
         if (hasResourceExtensionsFilter
            && !_options.AllowedPluginResourceExtensions.Contains(Path.GetExtension(entry.Name))
            && !_whitelistedResourceFiles.Contains(entry.Name)  // Static content specification
            )
         {
            _logger.LogWarning("File {FileName} is not allowed in plugin package.", entry.Name);
            continue;
         }

         string packagePath = Path.Combine(packageRootFolder, entry.FullName);
         Directory.CreateDirectory(Path.GetDirectoryName(packagePath)!);
         using Stream zipStream = entry.Open();
         using var fileStream = new FileStream(packagePath, FileMode.Create);
         await zipStream.CopyToAsync(fileStream).ConfigureAwait(false);
      }

      var pluginIstatus = new PluginStatus(manifest);
      _storedPlugins.Add(pluginIstatus);

      return pluginIstatus;
   }

   private PluginManifest ReadManifest(ZipArchive archive)
   {
      try
      {
         var manifestEntry = archive.Entries.FirstOrDefault(e => e.FullName == MANIFEST_FILE_NAME) ?? throw new InvalidPluginException($"Invalid package, missing {MANIFEST_FILE_NAME} file.");

         using Stream zipStream = manifestEntry.Open();
         using var reader = new StreamReader(zipStream);

         var content = reader.ReadToEnd();
         _logger.LogDebug("Manifest content: {ManifestContent}", content);

         var manifest = DeserializeManifest(content);

         //check manifest validity
         if (manifest.Signature == null) throw new InvalidPluginException($"Invalid package, {MANIFEST_FILE_NAME} file signature is missing.");
         if (string.IsNullOrEmpty(manifest.Signature.Name)) throw new InvalidPluginException($"Invalid package, {MANIFEST_FILE_NAME} file signature name is missing.");
         if (string.IsNullOrEmpty(manifest.Signature.Version)) throw new InvalidPluginException($"Invalid package, {MANIFEST_FILE_NAME} file signature version is missing.");
         if (string.IsNullOrEmpty(manifest.Signature.Author)) throw new InvalidPluginException($"Invalid package, {MANIFEST_FILE_NAME} file signature author is missing.");

         return manifest;
      }
      catch (Exception ex) when (ex is not InvalidPluginException)
      {
         _logger.LogError(ex, "Failed to read the Manifest");
         throw new InvalidPluginException($"Invalid package, {MANIFEST_FILE_NAME} file is invalid.", ex);
      }
   }

   public ValueTask DisablePackageAsync(PluginManifest manifest)
   {
      var pluginStatus = _storedPlugins.FirstOrDefault(p => p.Manifest.Signature == manifest.Signature && p.Status == PluginStatus.PackageStatus.Activated)
         ?? throw new InvalidOperationException("Cannot disable a package that is not activated.");

      pluginStatus.Status = PluginStatus.PackageStatus.PendingDisabled;

      UpdateEnabledPluginsFile();

      return ValueTask.CompletedTask;
   }

   public ValueTask ActivatePackageAsync(PluginManifest manifest)
   {
      var pluginStatus = _storedPlugins.FirstOrDefault(p => p.Manifest.Signature == manifest.Signature && p.Status is PluginStatus.PackageStatus.Available)
         ?? throw new InvalidOperationException("Can only enable package that are in Available status.");

      pluginStatus.Status = PluginStatus.PackageStatus.PendingActivated;

      UpdateEnabledPluginsFile();

      return ValueTask.CompletedTask;
   }

   private void UpdateEnabledPluginsFile()
   {
      var enabledPlugins = _storedPlugins
         .Where(p => p.Status is PluginStatus.PackageStatus.Activated or PluginStatus.PackageStatus.PendingActivated)
         .Select(p => p.Manifest.Signature.GetFullName()).ToList();

      var json = JsonSerializer.Serialize(enabledPlugins, new JsonSerializerOptions { WriteIndented = true });

      var packageRootFolder = _pluginFoldersProvider.GetPluginAssetsPath();

      if (!Directory.Exists(packageRootFolder))
         Directory.CreateDirectory(packageRootFolder);

      File.WriteAllText(_pluginFoldersProvider.GetPluginEnabledFilePath(), json);
   }

   public ValueTask<bool> RemovePackageAsync(PluginManifest manifest) => RemovePackageAsync(manifest.Signature.GetFullName());

   public ValueTask<bool> RemovePackageAsync(string packageName)
   {
      if (_storedPlugins.Any(p => p.Manifest.Signature.GetFullName() == packageName && p.Status is PluginStatus.PackageStatus.Activated))
         throw new InvalidOperationException("Cannot remove an active package.");

      var packageFolder = _pluginFoldersProvider.GetPluginInstallationRoot(packageName);
      if (!Directory.Exists(packageFolder))
      {
         if (_storedPlugins.FirstOrDefault(p => p.Manifest.Signature.GetFullName() == packageName) is PluginStatus pluginStatus)
            _storedPlugins.Remove(pluginStatus);
         _invalidPlugins.Remove(packageFolder);

         UpdateEnabledPluginsFile();
         return ValueTask.FromResult(false);
      }

      try
      {
         Directory.Delete(packageFolder, true);
         if (_storedPlugins.FirstOrDefault(p => p.Manifest.Signature.GetFullName() == packageName) is PluginStatus pluginStatus)
            _storedPlugins.Remove(pluginStatus);
         _invalidPlugins.Remove(packageFolder);
      }
      catch (Exception)
      {
         throw;
      }
      finally
      {
         UpdateEnabledPluginsFile();
      }

      return ValueTask.FromResult(true);
   }

   private PluginManifest DeserializeManifest(string manifestContent)
   {
      return JsonSerializer.Deserialize<PluginManifest>(manifestContent)
         ?? throw new InvalidPluginException($"Invalid package, {MANIFEST_FILE_NAME} file is empty.");
   }

   public async ValueTask LoadStoredPackagesAsync(IServiceCollection services)
   {
      if (_storeLoaded) return;

      try
      {
         //implements a slimsemaphore to lock an async method
         await _lock.WaitAsync().ConfigureAwait(false);

         if (_storeLoaded) return;

         //read the enabled-plugin.json file
         HashSet<string>? enabledPlugins = null;
         var enabledPluginFilePath = _pluginFoldersProvider.GetPluginEnabledFilePath();
         if (File.Exists(enabledPluginFilePath))
         {
            var content = await File.ReadAllTextAsync(enabledPluginFilePath).ConfigureAwait(false);
            enabledPlugins = JsonSerializer.Deserialize<IEnumerable<string>>(content)?.ToHashSet();
            if (enabledPlugins != null)
            {
               _logger.LogInformation("Enabled plugins: {EnabledPlugins}", string.Join(", ", enabledPlugins));
            }
         }
         else
         {
            _logger.LogWarning("Enabled plugins file not found: {EnabledPluginFilePath}", enabledPluginFilePath);
         }

         var pluginAssetsPath = _pluginFoldersProvider.GetPluginAssetsPath();
         if (!Directory.Exists(pluginAssetsPath))
         {
            _logger.LogWarning("No plugin package path found: {PluginPackagePath}", pluginAssetsPath);
            return;
         }

         foreach (string pluginFolder in Directory.GetDirectories(pluginAssetsPath))
         {
            // skip the special folder that contains the storage
            if (pluginFolder.EndsWith(AppConstants.SpecialFolders.STORAGE)) continue;

            PluginManifest manifest;
            try
            {
               var manifestFilePath = Path.Combine(pluginFolder, MANIFEST_FILE_NAME);
               if (!Directory.Exists(pluginFolder))
               {
                  _invalidPlugins[pluginFolder] = "Plugin manifest file not found";

                  _logger.LogError("Plugin manifest file not found: {ManifestFilePath}", manifestFilePath);
                  continue;
               }

               manifest = DeserializeManifest(File.ReadAllText(manifestFilePath));
            }
            catch (Exception ex) when (ex is not InvalidPluginException)
            {
               _invalidPlugins[pluginFolder] = $"Invalid package, {MANIFEST_FILE_NAME} file is invalid.";

               _logger.LogError(ex, "Invalid package, {MANIFEST_FILE_NAME} file is invalid.", MANIFEST_FILE_NAME);
               continue;
            }

            PluginStatus.PackageStatus packageStatus = PluginStatus.PackageStatus.Available;
            string? problem = null;

            if (enabledPlugins is not null && enabledPlugins.Contains(manifest.Signature.GetFullName()))
            {
               bool loaded = await LoadPluginAsync(manifest, services, out problem).ConfigureAwait(false);
               packageStatus = loaded ? PluginStatus.PackageStatus.Activated : PluginStatus.PackageStatus.Invalid;
            }
            else
            {
               packageStatus = PluginStatus.PackageStatus.Available;
            }

            _storedPlugins.Add(new(manifest) { Status = packageStatus, Problems = problem });
         }

         _storeLoaded = true;
      }
      catch (Exception ex)
      {
         _logger.LogError(ex, "Failed to load stored packages");
      }
      finally
      {
         UpdateEnabledPluginsFile(); //update the enabled-plugins.json file to fix eventual errors in enabled plugins manually moved or corrupted

         _lock.Release();
      }
   }


   /// <summary>
   /// Tries to load the plugin package.
   /// If something goes wrong while loading the package, the problem is returned in the <paramref name="problem"/> parameter.
   /// </summary>
   private ValueTask<bool> LoadPluginAsync(PluginManifest manifest, IServiceCollection services, [MaybeNullWhen(false)] out string problem)
   {
      try
      {
         var currentPluginStatus = _storedPlugins.FirstOrDefault(p => p.Manifest.Signature == manifest.Signature);
         if (currentPluginStatus?.Status == PluginStatus.PackageStatus.Activated)
         {
            problem = "Plugin already loaded";
            _logger.LogWarning("Plugin already loaded: {PluginName}", manifest.Signature.GetFullName());
            return ValueTask.FromResult(false);
         }

         var packageRoot = _pluginFoldersProvider.GetPluginInstallationRoot(manifest);

         if (!Directory.Exists(packageRoot))
         {
            _logger.LogError("Plugin package not found: {PackageRoot}", packageRoot);
            throw new InvalidPluginException($"Plugin package not found: {packageRoot}");
         }

         var entryPoint = manifest.Signature.Entry;
         if (string.IsNullOrEmpty(entryPoint))
         {
            _logger.LogError("Plugin package entry point not found: {PackageRoot}", packageRoot);
            throw new InvalidPluginException($"Plugin package entry point not found: {packageRoot}");
         }

         // check if the entry point exists (the plugin main dll)
         var entryPointPath = Path.Combine(packageRoot, entryPoint);
         if (!File.Exists(entryPointPath))
         {
            _logger.LogError("Plugin package entry point not found: {EntryPointPath}", entryPointPath);
            throw new InvalidPluginException($"Plugin package entry point not found: {entryPointPath}");
         }

         using var loader = PluginLoader.CreateFromAssemblyFile(entryPointPath, null, c => c.PreferSharedTypes = true);

         var pluginAssembly = loader.LoadDefaultAssembly();

         var pageDefinitions = FindAppPages(manifest, pluginAssembly);

         var loadedPackage = new Package(manifest, pluginAssembly, null)
         {
            Pages = pageDefinitions
         };

         //register the plugin services
         var pluginEntry = pluginAssembly.GetTypes().FirstOrDefault(t => t.IsAssignableTo(typeof(IPluginEntry)))
            ?? throw new InvalidPluginException($"Plugin entry point not found: {entryPointPath}. A plugin must have a class that implements {nameof(IPluginEntry)}");

         try
         {
            var pluginEntryInstance = Activator.CreateInstance(pluginEntry) as IPluginEntry;
            pluginEntryInstance?.Initialize(manifest, services);
         }
         catch (Exception ex)
         {
            throw new InvalidPluginException($"Failed to register plugin services: {entryPointPath}. {pluginEntry} must have a parameterless constructor and a valid RegisterServices implementation.", ex);
         }

         if (currentPluginStatus != null)
         {
            currentPluginStatus.Status = PluginStatus.PackageStatus.Activated;
         }

         _loadedPackages.Add(loadedPackage);
         PackageLoaded?.Invoke(this, loadedPackage);

         problem = null;

         UpdateEnabledPluginsFile();

         return ValueTask.FromResult(true);
      }
      catch (Exception ex)
      {
         problem = ex.Message;
         return ValueTask.FromResult(false);
      }
   }

   /// <summary>
   /// Find all valid pages in the plugin assembly and add them to the list of pages.
   /// </summary>
   /// <param name="manifest">The plugin manifest.</param>
   /// <param name="pluginAssembly">The plugin assembly.</param>
   /// <returns>Returns true if the plugin contains pages, false otherwise.</returns>
   private IReadOnlyList<PageDefinition> FindAppPages(PluginManifest manifest, Assembly pluginAssembly)
   {
      var pageDefinitions = _packageInspector.FindPageDefinitions(pluginAssembly);
      if (pageDefinitions.Count > 0)
      {
         //check that all loadedPackage doesn't contains duplicated Page routes
         var knownPages = _loadedPackages
            .SelectMany(p => p.Pages ?? Enumerable.Empty<PageDefinition>())
            .Select(p => p.Url)
            .ToHashSet();

         var duplicatesPageUrls = pageDefinitions.Where(p => knownPages.Contains(p.Url));
         if (duplicatesPageUrls.Any())
         {
            var duplicates = string.Join(';', duplicatesPageUrls.Select(p => p.Url));
            throw new InvalidPluginException($"Plugin {manifest.Signature.GetFullName()} contains duplicated page urls: {duplicates}");
         }

         _pluginPageAssemblies[manifest.Signature.GetFullName()] = pluginAssembly;
      }

      return pageDefinitions;
   }

   public void AddDebuggableModuleAssembly(Assembly pluginAssembly)
   {
      _pluginPageAssemblies[pluginAssembly.FullName!] = pluginAssembly;
   }
}

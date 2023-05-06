using System.Text;
using AIdentities.UI.Features.Core.Services.Plugins;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Internal;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Primitives;

namespace AIdentities.UI.Features.Core.Services.PluginStaticResources;

/// <summary>
/// Looks up files using the on-disk file system
/// </summary>
/// <remarks>
/// When the environment variable "DOTNET_USE_POLLING_FILE_WATCHER" is set to "1" or "true", calls to
/// <see cref="Watch(string)" /> will use <see cref="PollingFileChangeToken" />.
/// </remarks>
public class PluginStaticResourceProvider : IPluginStaticResourceProvider
{
   private static readonly char[] _pathSeparators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
   readonly ILogger<IPluginStaticResourceProvider> _logger;
   readonly IPluginFoldersProvider _pluginFoldersProvider;

   readonly string _root;

   /// <summary>
   /// Stores the association between the plugin root folder (given by it's entry assembly name) 
   /// and the absolute path to the plugin root folder.
   /// The key is the plugin assembly name
   /// The value is the absolute path to the plugin root folder.
   /// </summary>
   private readonly Dictionary<string, string> _pluginRoots = new();
   private bool _initialized;

   /// <summary>
   /// Initializes a new instance of a PhysicalFileProvider at the given root directory.
   /// </summary>
   /// <param name="root">The root directory. This should be an absolute path.</param>
   public PluginStaticResourceProvider(ILogger<IPluginStaticResourceProvider> logger,
                                       IPluginFoldersProvider pluginFoldersProvider)
   {
      _logger = logger;
      _pluginFoldersProvider = pluginFoldersProvider;

      _root = _pluginFoldersProvider.GetPluginAssetsPath();
   }

   public void InjectDependencies(ILogger<IPluginStaticResourceProvider> logger, IPluginFoldersProvider pluginFoldersProvider)
   {
      throw new NotImplementedException();
   }

   public void Initialize(IEnumerable<Package> loadedPackages)
   {
      if (_initialized) throw new InvalidOperationException("The plugin static resource provider has already been initialized.");

      foreach (var loadedPlugin in loadedPackages)
      {
         // When we do matches in GetFullPath, we want to only match full directory names.
         var pluginAssetFolder = PathUtils.EnsureTrailingSlash(_pluginFoldersProvider.GetPluginInstallationRoot(loadedPlugin.PluginManifest));

         if (!Directory.Exists(pluginAssetFolder))
         {
            _logger.LogWarning("The physical plugin root {Root} does not exist. The plugin could not serve static files from this location.", _root);
            continue;
         }

         _pluginRoots[loadedPlugin.Assembly.GetName().Name!] = pluginAssetFolder;
      }

      _initialized = true;
   }

   /// <summary>
   /// Returns the full path for the given <paramref name="path"/> if it is contained within the root directory.
   /// </summary>
   /// <param name="path">The path that is being validated.</param>
   /// <returns>The full path of the <paramref name="path"/> if it is contained within the root directory, otherwise, <see langword="null"/>.
   /// </returns>
   private string? GetFullPath(string path)
   {
      if (PathUtils.PathNavigatesAboveRoot(path)) return null;

      string fullPath;
      try
      {
         fullPath = Path.GetFullPath(Path.Combine(_root, path));
      }
      catch
      {
         return null;
      }

      if (!IsUnderneathRoot(fullPath)) return null;

      return fullPath;
   }

   /// <summary>
   /// Determines if the given <paramref name="fullPath"/> is contained within the root directory.
   /// </summary>
   /// <param name="fullPath"></param>
   /// <returns></returns>
   private bool IsUnderneathRoot(string fullPath) => fullPath.StartsWith(_root, StringComparison.OrdinalIgnoreCase);

   /// <summary>
   /// Locate a file at the given path by directly mapping path segments to physical directories.
   /// </summary>
   /// <param name="subpath">Relative path that identifies the file.</param>
   /// <returns>The file information. Caller must check <see cref="IFileInfo.Exists"/> property. </returns>
   public IFileInfo GetFileInfo(string subpath)
   {
      if (string.IsNullOrEmpty(subpath) || !PathUtils.IsValidFolder(subpath)) return new NotFoundFileInfo(subpath);

      subpath = subpath.TrimStart(_pathSeparators);

      if (subpath.StartsWith("_content"))
      {
         var parts = new StringTokenizer(subpath, _pathSeparators).GetEnumerator();
         parts.MoveNext();
         parts.MoveNext();
         var possiblePluginName = parts.Current.Value;
         if(possiblePluginName != null && _pluginRoots.TryGetValue(possiblePluginName, out string? pluginRoot))
         {
            //string possiblePluginAssetPath = Path.Combine(pluginRoot, subpath.Substring(10 + possiblePluginName.Length));
            //get the remaining parts into a string
            var remainingParts = new StringBuilder();
            while (parts.MoveNext())
            {
               remainingParts.Append(parts.Current.Value);
               remainingParts.Append('/');
            }

            var possiblePluginAssetPath = GetFullPath(Path.Combine(pluginRoot, "staticwebassets", remainingParts.ToString()[..^1]));

            if (File.Exists(possiblePluginAssetPath))
            {
               return new PhysicalFileInfo(new FileInfo(possiblePluginAssetPath));
            }
         }
      }

      // Absolute paths not permitted.
      if (Path.IsPathRooted(subpath)) return new NotFoundFileInfo(subpath);

      string? fullPath = GetFullPath(subpath);
      if (fullPath == null)
      {
         return new NotFoundFileInfo(subpath);
      }

      var fileInfo = new FileInfo(fullPath);

      return new PhysicalFileInfo(fileInfo);
   }

   /// <summary>
   /// Enumerate a directory at the given path, if any.
   /// </summary>
   /// <param name="subpath">A path under the root directory. Leading slashes are ignored.</param>
   /// <returns>
   /// Contents of the directory. Caller must check <see cref="IDirectoryContents.Exists"/> property. <see cref="NotFoundDirectoryContents" /> if
   /// <paramref name="subpath" /> is absolute, if the directory does not exist, or <paramref name="subpath" /> has invalid
   /// characters.
   /// </returns>
   public IDirectoryContents GetDirectoryContents(string subpath)
   {
      try
      {
         if (subpath == null || !PathUtils.IsValidFolder(subpath)) return NotFoundDirectoryContents.Singleton;

         // Relative paths starting with leading slashes are okay
         subpath = subpath.TrimStart(_pathSeparators);

         // Absolute paths not permitted.
         if (Path.IsPathRooted(subpath)) return NotFoundDirectoryContents.Singleton;

         string? fullPath = GetFullPath(subpath);
         if (fullPath == null || !Directory.Exists(fullPath)) return NotFoundDirectoryContents.Singleton;

         return new PhysicalDirectoryContents(fullPath, ExclusionFilters.None);
      }
      catch (DirectoryNotFoundException) { }
      catch (IOException) { }

      return NotFoundDirectoryContents.Singleton;
   }


   public IChangeToken Watch(string filter) => FakeChangeToken.Instance;

   public class FakeChangeToken : IChangeToken
   {
      public static IChangeToken Instance { get; } = new FakeChangeToken();

      public bool ActiveChangeCallbacks => true;
      public bool HasChanged => false;

      public IDisposable RegisterChangeCallback(Action<object> callback, object? state) => FakeDisposable.Instance;
   }

   public class FakeDisposable : IDisposable
   {
      public static IDisposable Instance { get; } = new FakeDisposable();

      public void Dispose() { }
   }
}

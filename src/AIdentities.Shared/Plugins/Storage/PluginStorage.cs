namespace AIdentities.Shared.Plugins.Storage;

public class PluginStorage : IPluginStorage
{
   readonly ILogger<PluginStorage> _logger;
   readonly PluginManifest _pluginManifest;
   readonly string _pluginStorageRoot;

   public PluginSignature Signature => _pluginManifest.Signature;

   public PluginStorage(ILogger<PluginStorage> logger, PluginManifest pluginManifest, string pluginStorageRoot)
   {
      _logger = logger;
      _pluginManifest = pluginManifest;
      _pluginStorageRoot = Path.Combine(pluginStorageRoot, pluginManifest.Signature.Name);

      if (!Directory.Exists(_pluginStorageRoot))
      {
         Directory.CreateDirectory(_pluginStorageRoot);
      }
   }

   public ValueTask<bool> DeleteAsync(string fileName)
   {
      var path = Path.Combine(_pluginStorageRoot, fileName);
      if (File.Exists(path))
      {
         File.Delete(path);
         return new ValueTask<bool>(true);
      }
      return new ValueTask<bool>(false);
   }

   public ValueTask<bool> ExistsAsync(string fileName)
   {
      var path = Path.Combine(_pluginStorageRoot, fileName);
      return new ValueTask<bool>(File.Exists(path));
   }

   public ValueTask<IEnumerable<string>> ListAsync()
   {
      var files = Directory.GetFiles(_pluginStorageRoot);
      return new ValueTask<IEnumerable<string>>(files);
   }

   public async ValueTask<string?> ReadAsync(string fileName)
   {
      var path = Path.Combine(_pluginStorageRoot, fileName);
      if (!File.Exists(path)) return null;

      return await File.ReadAllTextAsync(path).ConfigureAwait(false);
   }

   public async ValueTask WriteAsync(string fileName, string? content)
   {
      var path = Path.Combine(_pluginStorageRoot, fileName);
      await File.WriteAllTextAsync(path, content).ConfigureAwait(false);
   }

   public async ValueTask AppendAsync(string fileName, string? content)
   {
      var path = Path.Combine(_pluginStorageRoot, fileName);
      await File.AppendAllTextAsync(path, content).ConfigureAwait(false);
   }

   public async ValueTask<TContent?> ReadAsJsonAsync<TContent>(string fileName)
   {
      var content = await ReadAsync(fileName).ConfigureAwait(false);
      if (content == null) return default;

      return JsonSerializer.Deserialize<TContent>(content);
   }

   public ValueTask WriteAsJsonAsync<TContent>(string fileName, TContent? content)
      => WriteAsync(fileName, JsonSerializer.Serialize(content));
}

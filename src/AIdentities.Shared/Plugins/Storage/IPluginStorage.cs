using AIdentities.Shared.Features.Core.Abstracts;
using AIdentities.Shared.Features.Core.Services;

namespace AIdentities.Shared.Plugins.Storage;

/// <summary>
/// Allow a default mechanism for plugins to store data on their specific folder.
/// Ideally a plugin should use this interface to store data, in order to have a standardized
/// way to store data. Doing so, it's easy for users to backup and restore their data.
/// This interface is the one that gets registered in the DI container to have a single
/// instance for each plugin that can be resolved by the container.
/// </summary>
/// <typeparam name="TPluginEntry">The type of the plugin entry.</typeparam>
public interface IPluginStorage<TPluginEntry> : IPluginStorage
where TPluginEntry : IPluginEntry
{ }

/// <summary>
/// Allow a default mechanism for plugins to store data on their specific folder.
/// Ideally a plugin should use this interface to store data, in order to have a standardized
/// way to store data. Doing so, it's easy for users to backup and restore their data.
/// </summary>
public interface IPluginStorage
{
   /// <summary>
   /// The signature of the plugin that is using this storage.
   /// </summary>
   PluginSignature Signature { get; }

   /// <summary>
   /// Read the content of a file from the plugin's folder.
   /// </summary>
   /// <param name="fileName">The name of the file to read.</param>
   /// <returns>The content of the file.</returns>
   ValueTask<string?> ReadAsync(string fileName);

   /// <summary>
   /// Read the content of a file from the plugin's folder.
   /// </summary>
   /// <param name="fileName">The name of the file to read.</param>
   /// <returns>The content of the file.</returns>
   ValueTask<string[]?> ReadAllLinesAsync(string fileName);

   /// <summary>
   /// Store the content of a file in the plugin's folder.
   /// </summary>
   /// <param name="fileName">The name of the file to store.</param>
   /// <param name="content">The content of the file to store.</param>
   ValueTask WriteAsync(string fileName, string? content);

   /// <summary>
   /// Store lines of text in a file in the plugin's folder.
   /// </summary>
   /// <param name="fileName">The name of the file to store.</param>
   /// <param name="lines">The lines to store.</param>
   ValueTask WriteAllLinesAsync(string fileName, IEnumerable<string> lines);

   /// <summary>
   /// Append the content of a file in the plugin's folder.
   /// </summary>
   /// <param name="fileName">The name of the file to append content into.</param>
   /// <param name="content">The content to append.</param>
   /// <returns></returns>
   ValueTask AppendAsync(string fileName, string? content);

   /// <summary>
   /// Read the content of a file from the plugin's folder as a JSON file.
   /// </summary>
   /// <typeparam name="TContent">The type of the content to read.</typeparam>
   /// <param name="fileName">The name of the file to read.</param>
   /// <returns></returns>
   ValueTask<TContent?> ReadAsJsonAsync<TContent>(string fileName);

   /// <summary>
   /// Store the content of a file in the plugin's folder as a JSON file.
   /// </summary>
   /// <typeparam name="TContent">The type of the content to store.</typeparam>
   /// <param name="fileName">The name of the file to store.</param>
   /// <param name="content">The content of the file to store.</param>
   /// <returns></returns>
   ValueTask WriteAsJsonAsync<TContent>(string fileName, TContent? content);

   /// <summary>
   /// Delete a file from the plugin's folder.
   /// </summary>
   /// <param name="fileName">The name of the file to delete.</param>
   /// <returns>True if the file was deleted, false otherwise.</returns>
   ValueTask<bool> DeleteAsync(string fileName);

   /// <summary>
   /// Check if a file exists in the plugin's folder.
   /// </summary>
   /// <param name="fileName">The name of the file to check.</param>
   /// <returns>True if the file exists, false otherwise.</returns>
   ValueTask<bool> ExistsAsync(string fileName);

   /// <summary>
   /// List all the files in the plugin's folder.
   /// </summary>
   /// <returns>The list of files in the plugin's folder.</returns>
   ValueTask<IEnumerable<string>> ListAsync();

   /// <summary>
   /// Loads a plugin settings from the plugin's folder.
   /// This method shouldn't be called directly, but through the <see cref="IPluginSettingsManager.GetAsync{TPluginSettings}"/>.
   /// </summary>
   /// <typeparam name="TSettings">The type of the settings to load.</typeparam>
   /// <returns>The plugin settings. If the settings file doesn't exist, the default settings are returned.</returns>
   ValueTask<IPluginSettings> LoadSettingsAsync(Type settingsType, IPluginSettings defaultSettings);

   /// <summary>
   /// Stores a plugin settings in the plugin's folder.
   /// This method shouldn't be called directly, but through the <see cref="IPluginSettingsManager.SetAsync{TPluginSettings}(TPluginSettings)"/>.
   /// </summary>
   /// <typeparam name="TSettings">The type of the settings to store.</typeparam>
   /// <param name="settings">The settings to store.</param>
   ValueTask SaveSettingsAsync(IPluginSettings settings);

   /// <summary>
   /// Create a copy of a file in the plugin's folder.
   /// </summary>
   /// <param name="fileName">Name of the file to backup</param>
   /// <param name="backupFileName">Name of the backup file. If the file exists, it will be overwritten.</param>
   /// <returns>True if the file was backed up, false otherwise.</returns>
   ValueTask<bool> BackupFileAsync(string fileName, string backupFileName);
}

using AIdentities.Shared.Plugins;

namespace AIdentities.Shared.Features.Core.Services;

/// <summary>
/// This interface is used by plugins to perform some startup operations.
/// Need to be registered by calling <see cref="BasePluginEntry{TPluginEntry}.RegisterPluginStartupService"/>.
/// </summary>
public interface IPluginStartup
{
   /// <summary>
   /// This method is automatically called by the application after all plugins have been loaded.
   /// </summary>
   /// <returns></returns>
   /// <exception cref="NotImplementedException"></exception>
   ValueTask StartupAsync();
}

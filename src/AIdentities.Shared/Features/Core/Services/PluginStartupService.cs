using Microsoft.Extensions.DependencyInjection;

namespace AIdentities.Shared.Features.Core.Services;

/// <summary>
/// This is an hosted service that creates a scope and runs all the registered <see cref="IPluginStartupService"/>.
/// </summary>
public class PluginStartupService
{
   readonly IEnumerable<IPluginStartup> _pluginStartups;

   public PluginStartupService(IEnumerable<IPluginStartup> pluginStartups)
   {
      _pluginStartups = pluginStartups;
   }

   public async Task StartupPlugins()
   {
      var startupTasks = _pluginStartups
         .Select(startup => startup.StartupAsync().AsTask())
         .ToArray();

      await Task.WhenAll(startupTasks).ConfigureAwait(false);
   }
}

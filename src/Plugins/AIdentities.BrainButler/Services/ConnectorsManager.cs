using AIdentities.Shared.Features.Core.Services;

namespace AIdentities.BrainButler.Services;

public class ConnectorsManager : IConnectorsManager, IDisposable
{
   readonly ILogger<ConnectorsManager> _logger;
   readonly IEnumerable<ICompletionConnector> _completionConnectors;
   readonly IEnumerable<IConversationalConnector> _conversationalConnectors;
   readonly IPluginSettingsManager _pluginSettingsManager;
   private ICompletionConnector? _completionConnector;
   private IConversationalConnector? _conversationalConnector;

   public ConnectorsManager(ILogger<ConnectorsManager> logger, IEnumerable<ICompletionConnector> CompletionConnectors, IEnumerable<IConversationalConnector> ConversationalConnectors, IPluginSettingsManager pluginSettingsManager)
   {
      _logger = logger;
      _completionConnectors = CompletionConnectors;
      _conversationalConnectors = ConversationalConnectors;
      _pluginSettingsManager = pluginSettingsManager;

      _pluginSettingsManager.OnSettingsUpdated += OnSettingsUpdated;
      ApplySettings(_pluginSettingsManager.Get<BrainButlerSettings>());
   }

   private void OnSettingsUpdated(object? sender, IPluginSettings pluginSettings)
   {
      if (pluginSettings is not BrainButlerSettings settings) return;

      _logger.LogDebug("Settings updated, applying new settings.");
      ApplySettings(settings);
   }

   private void ApplySettings(BrainButlerSettings settings)
   {
      _completionConnector = _completionConnectors.FirstOrDefault(c => c.Enabled && c.Name == settings.DefaultCompletionConnector);
      _conversationalConnector = _conversationalConnectors.FirstOrDefault(c => c.Enabled && c.Name == settings.DefaultConversationalConnector);
   }

   public IEnumerable<ICompletionConnector> GetAllCompletionConnectors() => _completionConnectors;
   public IEnumerable<IConversationalConnector> GetAllConversationalConnectors() => _conversationalConnectors;
   public ICompletionConnector? GetCompletionConnector() => _completionConnector;
   public IConversationalConnector? GetConversationalConnector() => _conversationalConnector;

   public void Dispose()
   {
      _pluginSettingsManager.OnSettingsUpdated -= OnSettingsUpdated;
   }
}

using AIdentities.Shared.Features.Core.Components;
using AIdentities.Shared.Plugins.Connectors;

namespace AIdentities.BrainButler.Components;

public partial class Settings : BasePluginSettingsTab<BrainButlerSettings, Settings.State>
{
   [Inject] IConnectorsManager ConnectorsManager { get; set; } = default!;

   MudForm? _form;

   protected override void OnInitialized()
   {
      _state.Initialize(ConnectorsManager);
      base.OnInitialized();
   }

   protected override async ValueTask<bool> AreSettingsValid()
   {
      await _form!.Validate().ConfigureAwait(false);
      return _form.IsValid;
   }

   public override Task<BrainButlerSettings> PerformSavingAsync()
   {
      return Task.FromResult(new BrainButlerSettings()
      {
         DefaultCompletionConnector = _state.DefaultCompletionConnector?.Name,
         DefaultConversationalConnector = _state.DefaultConversationalConnector?.Name,
      });
   }

   public static bool IsConnectorDisabled(IEnumerable<IConnector> connectors, string? connectorName)
   {
      if (connectorName == null) return true;

      var connector = connectors.FirstOrDefault(x => x.Name == connectorName);
      return connector == null ? true : !connector.Enabled;
   }
}

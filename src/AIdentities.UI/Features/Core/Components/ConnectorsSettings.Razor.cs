using AIdentities.Shared.Plugins.Connectors.Conversational;
using Microsoft.AspNetCore.Components;

namespace AIdentities.UI.Features.Core.Components;

public partial class ConnectorsSettings : ComponentBase
{
   [Inject] ILogger<PluginsSettings> Logger { get; set; } = null!;
   [Inject] IEnumerable<IConversationalConnector> ConversationalConnectors { get; set; } = null!;
   [Inject] INotificationService NotificationService { get; set; } = null!;
   [Inject] IDialogService DialogService { get; set; } = null!;

   private MudForm? _form;

   protected override Task OnInitializedAsync()
   {
      return base.OnInitializedAsync();
   }

   protected override void OnInitialized()
   {
      base.OnInitialized();

      _state.ConversationalConnectors.AddRange(ConversationalConnectors);
   }

   void EditConnector(IConversationalConnector connector)
   {
      _state.PrepareConnectorForEdit(connector);
   }

   void Undo()
   {
      _state.PrepareConnectorForEdit(_state.SelectedConnector);
   }

   Task SaveConnectorSettings()
   {
      //to fix ASAP: IPluginStorage may be WRONG and take last registered plugin instead of the one we are editing
      !!!
      _state.SelectedConnector?.GetSettings .SetSettings(new ConnectorSettings()
      {
         EndPoint = new Uri(_state.ConnectorEndPoint!),
         ApiKey = _state.ConnectorApiKey
      });
   }
}

using AIdentities.Shared.Plugins.Connectors.Conversational;
using AIdentities.UI.Features.Core.Services.Plugins;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace AIdentities.UI.Features.Core.Components;

public partial class ConnectorsSettings : ComponentBase
{
   [Inject] ILogger<PluginsSettings> Logger { get; set; } = null!;
   [Inject] IEnumerable<IConversationalConnector> ConversationalConnectors { get; set; } = null!;
   [Inject] INotificationService NotificationService { get; set; } = null!;
   [Inject] IDialogService DialogService { get; set; } = null!;

   protected override Task OnInitializedAsync()
   {
      return base.OnInitializedAsync();
   }

   protected override void OnInitialized()
   {
      base.OnInitialized();

      _state.ConversationalConnectors.AddRange(ConversationalConnectors);
   }

}

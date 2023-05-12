using AIdentities.Shared.Plugins.Connectors.Conversational;
using Microsoft.AspNetCore.Components;

namespace AIdentities.UI.Features.Core.Components;

public partial class ConnectorsSettings : ComponentBase
{
   class State
   {
      /// <summary>
      /// List of uploaded packages.
      /// </summary>
      public List<IConversationalConnector> ConversationalConnectors { get; set; } = new();

      public IConversationalConnector? SelectedConnector { get; set; }

      public string? ConnectorEndPoint { get; set; }
      public string? ConnectorApiKey { get; set; }

      public void PrepareConnectorForEdit(IConversationalConnector? connector)
      {
         var settings = connector?.GetSettings();

         SelectedConnector = connector;
         ConnectorEndPoint = settings?.EndPoint?.ToString() ?? "";
         ConnectorApiKey = settings?.ApiKey?.ToString() ?? "";
      }
   }

   private readonly State _state = new State();
}

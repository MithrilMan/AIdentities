using AIdentities.Shared.Plugins.Connectors.Conversational;
using AIdentities.UI.Features.Core.Services.Plugins;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace AIdentities.UI.Features.Core.Components;

public partial class ConnectorsSettings : ComponentBase
{
   class State
   {
      /// <summary>
      /// List of uploaded packages.
      /// </summary>
      public List<IConversationalConnector> ConversationalConnectors { get; set; } = new();

      
   }

   private readonly State _state = new State();
}

using AIdentities.Connector.OpenAI.Components;
using AIdentities.Connector.OpenAI.Models;
using AIdentities.Connector.OpenAI.Services;
using AIdentities.Shared.Plugins.Connectors.Completion;

namespace AIdentities.Connector.OpenAI;

public class PluginEntry : BasePluginEntry<PluginEntry>
{
   public override void RegisterServices(IServiceCollection services)
   {
      services.AddScoped<IConversationalConnector, OpenAIChatConnector>();
      services.AddScoped<ICompletionConnector, OpenAICompletionConnector>();

      RegisterPluginSettings<OpenAISettings, Settings>("OpenAi");
   }
}

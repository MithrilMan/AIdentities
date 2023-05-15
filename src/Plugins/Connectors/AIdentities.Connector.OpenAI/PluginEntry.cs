using AIdentities.Connector.OpenAI.Components;
using AIdentities.Connector.OpenAI.Models;
using AIdentities.Connector.OpenAI.Services;

namespace AIdentities.Connector.OpenAI;

public class PluginEntry : BasePluginEntry
{
   public override void RegisterServices(IServiceCollection services)
   {
      services.AddScoped<IConversationalConnector, OpenAIConnector>();

      RegisterPluginSettings<OpenAISettings, Settings>("OpenAi");
   }
}

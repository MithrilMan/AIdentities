using AIdentities.Connector.OpenAI.Services;

namespace AIdentities.Connector.OpenAI;

public class PluginEntry : BasePluginEntry
{
   public override void RegisterServices(IServiceCollection services)
   {
      services
         .AddScoped<OpenAISettings>()
         .AddScoped<IConversationalConnectorSettings<OpenAIConnector>, OpenAISettings>(sp => sp.GetRequiredService<OpenAISettings>());

      services.AddScoped<IConversationalConnector, OpenAIConnector>();
   }
}

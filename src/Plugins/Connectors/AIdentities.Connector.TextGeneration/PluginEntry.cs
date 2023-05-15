namespace AIdentities.Connector.TextGeneration;

public class PluginEntry : BasePluginEntry
{
   public override void RegisterServices(IServiceCollection services)
   {
      services.AddScoped<IConversationalConnector, TextGenerationConnector>();

      RegisterPluginSettings<TextGenerationSettings, Settings>("TextGeneration");
   }
}

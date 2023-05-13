namespace AIdentities.Connector.OpenAI;

public sealed class DebuggableModule : BaseDebuggableModule<PluginEntry>
{
   protected override void RegisterServices(IServiceCollection services, IHostEnvironment hostEnvironment)
   {
      // important, to expose this assembly when referencing this plugin directly in the AIdentities project.
      services.AddSingleton<IDebuggableModule, DebuggableModule>();
   }
}

namespace AIdentities.UI;

/// <summary>
/// This class was created to allow debuggable modules to register their services in a more comfortable way.
/// The only file you have to change is this one, instead of changing the DependencyInjection file.
/// </summary>
public static class DebuggableModulesRegistration
{
   /// <summary>
   /// In order to let debuggable modules register their services, we need to register them.
   /// This is a manual process, since we cannot use the plugin manager to do this for us.
   /// So everytime we want to develop a plugin but we want to take advantage of the debuggable modules in a more
   /// comfortable way, we need to register them here.
   /// </summary>
   /// <param name="services">The service collection where to register plugin services.</param>
   public static void RegisterDebuggableModules(IServiceCollection services)
   {
      services.AddSingleton<IDebuggableModule, Chat.DebuggableModule>();
      services.AddSingleton<IDebuggableModule, BooruAIdentityImporter.DebuggableModule>();
      services.AddSingleton<IDebuggableModule, Connector.OpenAI.DebuggableModule>();
      services.AddSingleton<IDebuggableModule, Connector.TextGeneration.DebuggableModule>();
      services.AddSingleton<IDebuggableModule, BrainButler.DebuggableModule>();
   }
}

using Microsoft.Extensions.DependencyInjection;

namespace AIdentities.BrainButler;
public class PluginEntry : BasePluginEntry
{
   public override void RegisterServices(IServiceCollection services)
   {
      services.AddScoped<IBrainButlerCommandManager, BrainButlerCommandManager>();
      services.AddScoped<IPromptGenerator, PromptGenerator>();

      RegisterAvailableCommands(services);
   }

   private void RegisterAvailableCommands(IServiceCollection services)
   {
      services
         .AddScoped<IBrainButlerCommand, ChangeTheme>();
   }
}

using AIdentities.BrainButler.Commands.ChangeTheme;
using AIdentities.BrainButler.Commands.WhatTimeIsIt;
using Microsoft.Extensions.DependencyInjection;

namespace AIdentities.BrainButler;
public class PluginEntry : BasePluginEntry<PluginEntry>
{
   public override void RegisterServices(IServiceCollection services)
   {
      services.AddScoped<IBrainButlerCommandManager, BrainButlerCommandManager>();
      services.AddScoped<IPromptGenerator, PromptGenerator>();
      services.AddScoped<IConnectorsManager, ConnectorsManager>();
      services.AddScoped<ThemeUpdater>();

      RegisterPluginStartup<StartupService>();
      RegisterPluginSettings<BrainButlerSettings, Settings>("Brain Butler");
      RegisterAvailableCommands(services);
   }

   private void RegisterAvailableCommands(IServiceCollection services)
   {
      services
         .AddScoped<IBrainButlerCommand, ChangeTheme>()
         .AddScoped<IBrainButlerCommand, WhatTimeIsIt>();
   }
}

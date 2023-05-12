using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AIdentities.BrainWittle;

public sealed class DebuggableModule : BaseDebuggableModule<PluginEntry>
{
   public override string Name => "Chat";

   protected override void RegisterServices(IServiceCollection services, IHostEnvironment hostEnvironment)
   {
      // important, to expose this assembly when referencing this plugin directly in the AIdentities project.
      services.AddSingleton<IDebuggableModule, DebuggableModule>();
   }
}

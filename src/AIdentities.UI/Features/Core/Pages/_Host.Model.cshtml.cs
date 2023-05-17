using AIdentities.UI.Features.Core.Services.Plugins;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AIdentities.UI.Features.Core.Pages;
public class HostModel : PageModel
{
   readonly ILogger<HostModel> _logger;
   readonly IPluginManager _pluginManager;

   //private void ProcessHostResources(Assembly assembly, Alias alias)
   //{
   //   var types = assembly.GetTypes().Where(item => item.GetInterfaces().Contains(typeof(IHostResources)));
   //   foreach (var type in types)
   //   {
   //      var obj = Activator.CreateInstance(type) as IHostResources;
   //      foreach (var resource in obj.Resources)
   //      {
   //         resource.Level = ResourceLevel.App;
   //         ProcessResource(resource, 0, alias);
   //      }
   //   }
   //}

   public HostModel(ILogger<HostModel> logger, IPluginManager pluginManager)
   {
      _logger = logger;
      _pluginManager = pluginManager;
   }

   public IActionResult OnGet()
   {
      InjectPluginResources();

      return Page();
   }

   private void InjectPluginResources()
   {
      //var packages = _pluginManager.LoadedPackages;
      //foreach (var package in packages)
      //{
      //   package.PluginManifest.Tags

      //   var alias = new Alias(package.Name, package.Name);
      //   var assembly = package.Assembly;
      //   var types = assembly.GetTypes().Where(item => item.GetInterfaces().Contains(typeof(IHostResources)));
      //   foreach (var type in types)
      //   {
      //      var obj = Activator.CreateInstance(type) as IHostResources;
      //      foreach (var resource in obj.Resources)
      //      {
      //         resource.Level = ResourceLevel.App;
      //         ProcessResource(resource, 0, alias);
      //      }
      //   }
      //}
   }
}

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MudBlazor;
using System.Reflection;

namespace AIdentities.UI.Features.Core.Pages;
public class HostModel : PageModel
{
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

   public IActionResult Test()
   {
      Console.WriteLine("TEST");
      return null;
   }
}

namespace AIdentities.UI.Features.Core.Services.PageManager;

public class PageDefinitionProvider : IPageDefinitionProvider, IDisposable
{
   readonly ILogger<PageDefinitionProvider> _logger;
   readonly IEventBus _eventBus;
   readonly IPackageInspector _packageInspector;
   readonly IPluginManager _pluginManager;

   readonly HashSet<PageDefinition> _pages = new();

   public PageDefinitionProvider(ILogger<PageDefinitionProvider> logger,
                                 IPluginManager pluginManager,
                                 IEventBus eventBus,
                                 IDebuggablePagesManager debuggablePagesManager,
                                 IPackageInspector packageInspector)
   {
      _logger = logger;
      _pluginManager = pluginManager;
      _eventBus = eventBus;
      _packageInspector = packageInspector;

      _eventBus.Subscribe(this);
      _pluginManager.PackageLoaded += async (sender, package) => await OnPackageLoaded(sender, package).ConfigureAwait(false);

      foreach (var pageDefinition in _pluginManager.LoadedPackages.SelectMany(p => p.Pages ?? Enumerable.Empty<PageDefinition>()))
      {
         _pages.Add(pageDefinition);
      }

      // load internal pages
      foreach (var internalPage in _packageInspector.FindPageDefinitions(GetType().Assembly))
      {
         _pages.Add(internalPage);
      }

      // loads pages from debuggable modules
      foreach (var pageDefinition in debuggablePagesManager.FindPageDefinitions())
      {
         _pages.Add(pageDefinition);
      }
   }

   public IEnumerable<PageDefinition> GetPageDefinitions() => _pages;

   private async Task OnPackageLoaded(object? sender, Package loadedPackage)
   {
      if (loadedPackage.Pages is { Count: > 0 })
      {
         foreach (var page in loadedPackage.Pages)
         {
            if (_pages.Add(page))
               _logger.LogInformation("Page {PageName} added", page.Title);
         }

         await _eventBus.Publish(new PageDefinitionsAdded(loadedPackage.Pages)).ConfigureAwait(false);
      }
   }

   public void Dispose()
   {
      _eventBus.Unsubscribe(this);
      _pluginManager.PackageLoaded -= async (sender, package) => await OnPackageLoaded(sender, package).ConfigureAwait(false);
   }
}

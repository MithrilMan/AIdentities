namespace AIdentities.UI.Features.Core.Services.PageManager;

public interface IPageDefinitionProvider
{
   IEnumerable<PageDefinition> GetPageDefinitions();
}

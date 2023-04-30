using AIdentities.UI.Features.Core.Services.PageManager;
using Microsoft.AspNetCore.Components;

namespace AIdentities.UI.Features.Core.Components;

public partial class AvailablePages : ComponentBase, IDisposable, IHandle<PageDefinitionsAdded>
{
   [Inject] ILogger<AvailablePages> Logger { get; set; } = null!;
   [Inject] private IPageDefinitionProvider PageDefinitionProvider { get; set; } = default!;
   [Inject] private IEventBus EventBus { get; set; } = default!;

   protected override void OnInitialized()
   {
      base.OnInitialized();
      EventBus.Subscribe(this);
   }

   public Task HandleAsync(PageDefinitionsAdded message) => InvokeAsync(StateHasChanged);

   public void Dispose()
   {
      EventBus.Unsubscribe(this);
   }
}

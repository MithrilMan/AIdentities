using AIdentities.Shared.Plugins.Connectors.Conversational;
using Microsoft.AspNetCore.Components.Web;

namespace AIdentities.BrainWittle.Pages;

[Route(PAGE_URL)]
[PageDefinition(PAGE_TITLE, Icons.Material.Filled.Assistant, PAGE_URL, Description = "Allows the user to interact with BrainWittle, the personalized assistant.")]
public partial class Interaction : AppPage<Interaction>
{
   const string PAGE_TITLE = "BrainWittle";
   const string PAGE_URL = "brain-wittle";
   const string LIST_ID = "message-list-wrapper";
   const string LIST_SELECTOR = $"#{LIST_ID}";

   [Inject] private IDialogService DialogService { get; set; } = null!;
   [Inject] private IEnumerable<IConversationalConnector> Connectors { get; set; } = null!;
   [Inject] private IScrollService ScrollService { get; set; } = null!;

   private IConversationalConnector? _currentConnector;

   protected override void OnInitialized()
   {
      base.OnInitialized();
      _currentConnector = Connectors.FirstOrDefault();
   }

   private async Task ScrollToEndOfMessageList()
   {
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);
      await ScrollService.ScrollToBottom(LIST_SELECTOR).ConfigureAwait(false);
   }
}

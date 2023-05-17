using Microsoft.JSInterop;

namespace AIdentities.BrainButler.Components;

public partial class Message : ComponentBase
{
   [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
   [Inject] public IAIdentityProvider AIdentityProvider { get; set; } = default!;

   [Parameter] public ConversationPiece ChatMessage { get; set; } = default!;
   [Parameter] public bool IsSelected { get; set; } = default!;

   private async Task CopyToClipboard()
   {
      await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", ChatMessage.Message).ConfigureAwait(false);
   }
}

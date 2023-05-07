using Microsoft.JSInterop;

namespace AIdentities.Chat.Components;

public partial class Message : ComponentBase
{
   [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
   [Inject] public IAIdentityProvider AIdentityProvider { get; set; } = default!;

   [Parameter] public ChatMessage ChatMessage { get; set; } = default!;
   [Parameter] public bool IsSelected { get; set; } = default!;
   [Parameter] public EventCallback OnResend { get; set; }

   private async Task CopyToClipboard()
   {
      await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", ChatMessage.Message).ConfigureAwait(false);
   }

   Task Resend() => OnResend.InvokeAsync();
}

using Microsoft.JSInterop;

namespace AIdentities.Chat.Components;

public partial class GenerativeChatMessage : ComponentBase
{
   [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
   [Inject] public IAIdentityProvider AIdentityProvider { get; set; } = default!;

   [Parameter] public ConversationMessage Message { get; set; } = default!;
   [Parameter] public bool NewGroup { get; set; } = true;
   [Parameter] public bool IsSelected { get; set; } = default!;
   [Parameter] public EventCallback<ConversationMessage> OnDelete { get; set; }
   [Parameter] public EventCallback<ConversationMessage> OnPlayAudio { get; set; }
   [Parameter] public EventCallback<ConversationMessage> OnStopAudio { get; set; }

   private bool _isPlaying;

   private async Task CopyToClipboard()
   {
      await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", Message.Text).ConfigureAwait(false);
   }

   Task Delete() => OnDelete.InvokeAsync(Message);

   //async Task PlayAudio()
   //{
   //   _isPlaying = true;
   //   _ = InvokeAsync(StateHasChanged,).ConfigureAwait(false);

   //   await OnPlayAudio.InvokeAsync(Message).ConfigureAwait(false);

   //   _isPlaying = false;
   //   await InvokeAsync(StateHasChanged).ConfigureAwait(false);
   //}

   Task PlayAudio() => OnPlayAudio.InvokeAsync(Message);
   
   async Task StopAudio()
   {
      await OnStopAudio.InvokeAsync(Message).ConfigureAwait(false);
   }
}

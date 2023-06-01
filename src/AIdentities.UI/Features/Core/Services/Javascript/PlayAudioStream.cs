using Microsoft.JSInterop;

namespace AIdentities.UI.Features.Core.Services.Javascript;

public class PlayAudioStream : IPlayAudioStream
{
   private readonly IJSRuntime _jsRuntime;

   public PlayAudioStream(IJSRuntime jsRuntime)
   {
      _jsRuntime = jsRuntime;
   }

   public async Task PlayAudioFileStream(DotNetStreamReference streamReference)
   {
      await _jsRuntime.InvokeVoidAsync("playAudioFileStream", streamReference).ConfigureAwait(false);
   }
}

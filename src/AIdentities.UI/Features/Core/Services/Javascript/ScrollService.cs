using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AIdentities.UI.Features.Core.Services.Javascript;

public class ScrollService : IScrollService
{
   private readonly IJSRuntime _jsRuntime;

   public ScrollService(IJSRuntime jsRuntime)
   {
      _jsRuntime = jsRuntime;
   }

   public async Task ScrollToTop(ElementReference element)
   {
      await _jsRuntime.InvokeVoidAsync("scrollElementToTop", element).ConfigureAwait(false);
   }

   public async Task ScrollToBottom(ElementReference element)
   {
      await _jsRuntime.InvokeVoidAsync("scrollElementToBottom", element).ConfigureAwait(false);
   }

   public async Task ScrollToTop(string selector)
   {
      await _jsRuntime.InvokeVoidAsync("scrollElementToBottom", selector).ConfigureAwait(false);
   }

   public async Task ScrollToBottom(string selector)
   {
      await _jsRuntime.InvokeVoidAsync("scrollElementToBottom", selector).ConfigureAwait(false);
   }

   public async Task EnsureIsVisible(string selector)
   {
      await _jsRuntime.InvokeVoidAsync("ensureIsVisible", selector).ConfigureAwait(false);
   }
   
}

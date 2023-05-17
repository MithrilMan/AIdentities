using System.Text;
using System.Text.Json;
using Microsoft.JSInterop;

namespace AIdentities.UI.Features.Core.Services.Javascript;

public class DownloadService : IDownloadService
{
   private readonly IJSRuntime _jsRuntime;

   public DownloadService(IJSRuntime jsRuntime)
   {
      _jsRuntime = jsRuntime;
   }

   public async Task DownloadFileAsJsonAsync<TContent>(string fileName, TContent content, JsonSerializerOptions? options = null)
   {
      using var stream = new MemoryStream();
      await JsonSerializer.SerializeAsync(stream, content, options).ConfigureAwait(false);
      stream.Position = 0;
      using var streamRef = new DotNetStreamReference(stream);
      await _jsRuntime.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef).ConfigureAwait(false);
   }

   public async Task DownloadFileFromStreamAsync(string fileName, string content)
   {
      using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
      stream.Position = 0;
      using var streamRef = new DotNetStreamReference(stream);
      await _jsRuntime.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef).ConfigureAwait(false);
   }

   public async Task DownloadFileFromStreamAsync(string fileName, Stream content)
   {
      using var streamRef = new DotNetStreamReference(content);
      await _jsRuntime.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef).ConfigureAwait(false);
   }
}

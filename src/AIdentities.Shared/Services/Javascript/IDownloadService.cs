namespace AIdentities.Shared.Services.Javascript;

/// <summary>
/// Service that allows to download files from the browser.
/// </summary>
public interface IDownloadService
{
   /// <summary>
   /// Downloads the given file with the specified filename, reading the content from the given stream.
   /// </summary>
   /// <param name="fileName">The name of the file to download.</param>
   /// <param name="content">The content of the file to download.</param>
   Task DownloadFileFromStreamAsync(string fileName, Stream content);

   /// <summary>
   /// Downloads the given file with the specified filename, reading the content from the given stream.
   /// </summary>
   /// <param name="fileName">The name of the file to download.</param>
   /// <param name="content">The content of the file to download.</param>
   Task DownloadFileFromStreamAsync(string fileName, string content);

   /// <summary>
   /// Downloads the given file with the specified filename, serializing the content to JSON.
   /// </summary>
   /// <typeparam name="TContent">The type of the content to serialize.</typeparam>
   /// <param name="fileName">The name of the file to download.</param>
   /// <param name="content">The content of the file to download.</param>
   /// <param name="options">Optional json serializer options.</param>
   /// <returns></returns>
   Task DownloadFileAsJsonAsync<TContent>(string fileName, TContent content, JsonSerializerOptions? options = null);
}

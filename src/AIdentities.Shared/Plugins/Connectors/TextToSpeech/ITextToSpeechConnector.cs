namespace AIdentities.Shared.Plugins.Connectors.TextToSpeech;

/// <summary>
/// To be used for endpoints that support Text To Speech.
/// </summary>
public interface ITextToSpeechConnector : IConnector
{
   Task<ITextToSpeechResponse> RequestTextToSpeechAsync(ITextToSpeechRequest request, CancellationToken cancellationToken);

   /// <summary>
   /// Returns the audio stream of the text to speech conversion.
   /// </summary>
   /// <param name="request">The request to be sent to the connector.</param>
   /// <param name="cancellationToken">The cancellation token.</param>
   /// <returns>
   /// The audio stream. It is the responsibility of the caller to dispose the stream.
   /// </returns>
   Task RequestTextToSpeechAsStreamAsync(ITextToSpeechRequest request, Func<Stream, Task> streamConsumer, CancellationToken cancellationToken);
}

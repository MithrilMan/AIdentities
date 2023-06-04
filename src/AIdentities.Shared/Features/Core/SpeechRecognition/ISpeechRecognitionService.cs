namespace AIdentities.Shared.Features.Core.SpeechRecognition;

/// <summary>
/// A service the exposes various JavaScript interop capabilities specific to the
/// <c>speechRecognition</c> APIs. See <a href="https://developer.mozilla.org/docs/Web/API/SpeechRecognition"></a>
/// </summary>
public interface ISpeechRecognitionService : IAsyncDisposable
{
   /// <summary>
   /// Initializes the speech recognition.
   /// </summary>
   /// <param name="speechRecognitionListeners">The listeners to use for speech recognition.</param>
   Task InitializeSpeechRecognitionAsync(ISpeechRecognitionListener speechRecognitionListeners);

   /// <summary>
   /// Starts a speech recognition session.
   /// </summary>
   /// <param name="language">The language to use for speech recognition.</param>
   /// <param name="continuous">Whether to return each recognition result.</param>
   /// <param name="interimResults">Whether to return interim results.</param>
   Task StartSpeechRecognitionAsync(string language, bool continuous, bool interimResults);

   /// <summary>
   /// Cancels the active speech recognition session.
   /// </summary>
   /// <param name="isAborted">
   /// Is aborted controls which API to call,
   /// either <c>speechRecognition.stop</c> or <c>speechRecognition.abort</c>.
   /// </param>
   Task CancelSpeechRecognitionAsync(bool isAborted);
}

namespace AIdentities.Shared.Features.Core.SpeechRecognition;

public interface ISpeechRecognitionListener
{
   /// <summary>
   /// Called when a voice recognition result is received.
   /// First paramenter is the transcript, second parameter is whether the transcript is final.
   /// </summary>
   Task OnVoiceRecognized(string transcript, bool isFinal);

   /// <summary>
   /// Called when a voice recognition session is started.
   /// </summary>
   Task OnVoiceRecognitionStarted();

   /// <summary>
   /// Called when a voice recognition session is finished.
   /// </summary>
   Task OnVoiceRecognitionFinished();

   /// <summary>
   /// Called when a voice recognition error occurs.
   /// </summary>
   /// <param name="error">The error that occurred.</param>
   Task OnVoiceRecognitionError(SpeechRecognitionError error);
}

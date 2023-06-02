using Microsoft.JSInterop;

namespace AIdentities.Shared.Services.Javascript;

/// <summary>
/// Service to scroll elements into view.
/// </summary>
public interface IPlayAudioStream
{
   /// <summary>
   /// Plays an audio stream.
   /// Supported formats is audio/mpeg.
   /// </summary>
   /// <param name="streamReference">The stream containing the sound to play.</param>
   /// <returns></returns>
   Task PlayAudioFileStream(DotNetStreamReference streamReference);

   /// <summary>
   /// Stops all the audio files playing on the page.
   /// </summary>
   /// <returns></returns>
   Task StopAudioFiles();
}

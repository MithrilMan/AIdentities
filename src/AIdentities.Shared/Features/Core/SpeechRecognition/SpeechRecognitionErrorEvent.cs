namespace AIdentities.Shared.Features.Core.SpeechRecognition;

/// <summary>
/// The error event that is fired when a speech recognition error occurs.
/// </summary>
/// <param name="Error">The error's name.</param>
/// <param name="Message">The error's message.</param>
public record SpeechRecognitionErrorEvent(string Error, string Message)
{
   [JsonPropertyName("error")]
   public string Error { get; set; } = Error;

   [JsonPropertyName("message")]
   public string Message { get; set; } = Message;
}

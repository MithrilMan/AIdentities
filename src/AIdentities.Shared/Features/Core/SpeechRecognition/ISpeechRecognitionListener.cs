namespace AIdentities.Shared.Features.Core.SpeechRecognition;

public interface ISpeechRecognitionListener
{
   public Func<string, Task> OnRecognized { get; }
   public Func<Task>? OnStarted { get; }
   public Func<Task>? OnFinished { get; }
   public Func<SpeechRecognitionErrorEvent, Task>? OnError { get; }
}

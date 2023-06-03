using Microsoft.JSInterop;

namespace AIdentities.Shared.Features.Core.SpeechRecognition;

internal sealed class BrowserSpeechRecognitionService : ISpeechRecognitionService, IAsyncDisposable
{
   private ISpeechRecognitionListener? _speechRecognitionListener;
   private bool _isRecognizing;
   private IJSObjectReference _module = default!;
   readonly IJSRuntime _jsRuntime;

   readonly object _lock = new object();

   public BrowserSpeechRecognitionService(IJSRuntime jsRuntime) => _jsRuntime = jsRuntime;

   /// <inheritdoc />
   public async Task InitializeSpeechRecognitionAsync(ISpeechRecognitionListener speechRecognitionListener)
   {
      if (speechRecognitionListener is null) throw new ArgumentNullException(nameof(speechRecognitionListener));

      _speechRecognitionListener = speechRecognitionListener;

      _module ??= await _jsRuntime.InvokeAsync<IJSObjectReference>(
             identifier: "import",
             args: "/_content/AIDentities.Shared/scripts/speech-recognition.js"
             ).ConfigureAwait(false);
   }

   /// <inheritdoc />
   public async Task CancelSpeechRecognitionAsync(bool isAborted)
   {
      if (_speechRecognitionListener is null) throw new InvalidOperationException("Browser speech recognition service is not initialized.");

      await _module.InvokeVoidAsync("cancelSpeechRecognition", isAborted).ConfigureAwait(false);
   }

   /// <inheritdoc />
   public async Task StartSpeechRecognitionAsync(string language, bool continuous, bool interimResults)
   {
      if (_speechRecognitionListener is null) throw new InvalidOperationException("Browser speech recognition service is not initialized.");

      if (_isRecognizing) throw new InvalidOperationException("Browser speech recognition service is already recognizing.");
      lock (_lock)
      {
         if (_isRecognizing) throw new InvalidOperationException("Browser speech recognition service is already recognizing.");
         _isRecognizing = true;
      }

      await _module.InvokeVoidAsync(
          "startSpeechRecognition",
          DotNetObjectReference.Create(this),
          language,
          continuous,
          interimResults,
          nameof(OnSpeechRecongizedAsync),
          nameof(OnRecognitionErrorAsync),
          nameof(OnStartedAsync),
          nameof(OnFinishedAsync)).ConfigureAwait(false);
   }

   [JSInvokable]
   public async Task OnStartedAsync()
   {
      if (!_isRecognizing) return;
      if (_speechRecognitionListener?.OnStarted is null) return;

      await _speechRecognitionListener.OnStarted().ConfigureAwait(false);
   }

   [JSInvokable]
   public async Task OnFinishedAsync()
   {
      if (!_isRecognizing) return;
      if (_speechRecognitionListener?.OnFinished is null) return;

      await _speechRecognitionListener.OnFinished().ConfigureAwait(false);
   }

   [JSInvokable]
   public async Task OnRecognitionErrorAsync(SpeechRecognitionErrorEvent errorEvent)
   {
      if (!_isRecognizing) return;
      if (_speechRecognitionListener?.OnError is null) return;

      await _speechRecognitionListener.OnError(errorEvent).ConfigureAwait(false);
   }

   [JSInvokable]
   public async Task OnSpeechRecongizedAsync(string transcript, bool isFinal)
   {
      if (!_isRecognizing) return;
      if (_speechRecognitionListener?.OnRecognized is null) return;

      if (isFinal)
         await _speechRecognitionListener.OnRecognized(transcript).ConfigureAwait(false);
   }

   async ValueTask IAsyncDisposable.DisposeAsync()
   {
      await CancelSpeechRecognitionAsync(true).ConfigureAwait(false);

      if (_module != null)
         await _module.DisposeAsync().ConfigureAwait(false);
   }
}

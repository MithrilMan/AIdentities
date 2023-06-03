using System.Diagnostics.CodeAnalysis;
using Microsoft.JSInterop;

namespace AIdentities.Shared.Features.Core.SpeechRecognition;

internal sealed class BrowserSpeechRecognitionService : ISpeechRecognitionService, IAsyncDisposable
{
   private ISpeechRecognitionListener? _speechRecognitionListener;
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

      await CancelSpeechRecognitionAsync(isAborted: true).ConfigureAwait(false);
   }

   /// <inheritdoc />
   public async Task CancelSpeechRecognitionAsync(bool isAborted)
   {
      EnsureIsInitialized();

      await _module.InvokeVoidAsync("cancelSpeechRecognition", isAborted).ConfigureAwait(false);
   }

   /// <inheritdoc />
   public async Task StartSpeechRecognitionAsync(string language, bool continuous, bool interimResults)
   {
      EnsureIsInitialized();

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
      EnsureIsInitialized();

      await _speechRecognitionListener.OnVoiceRecognitionStarted().ConfigureAwait(false);
   }

   [JSInvokable]
   public async Task OnFinishedAsync()
   {
      EnsureIsInitialized();

      await _speechRecognitionListener.OnVoiceRecognitionFinished().ConfigureAwait(false);
   }

   [JSInvokable]
   public async Task OnRecognitionErrorAsync(SpeechRecognitionError errorEvent)
   {
      EnsureIsInitialized();

      await _speechRecognitionListener.OnVoiceRecognitionError(errorEvent).ConfigureAwait(false);
   }

   [JSInvokable]
   public async Task OnSpeechRecongizedAsync(string transcript, bool isFinal)
   {
      EnsureIsInitialized();

      //if (isFinal)
      //{
      await _speechRecognitionListener.OnVoiceRecognized(transcript, isFinal).ConfigureAwait(false);
      //}
   }

   [MemberNotNull(nameof(_speechRecognitionListener))]
   private void EnsureIsInitialized()
   {
      if (_speechRecognitionListener is null) throw new InvalidOperationException("Browser speech recognition service is not initialized.");
   }

   async ValueTask IAsyncDisposable.DisposeAsync()
   {
      await CancelSpeechRecognitionAsync(true).ConfigureAwait(false);

      if (_module != null)
         await _module.DisposeAsync().ConfigureAwait(false);
   }
}

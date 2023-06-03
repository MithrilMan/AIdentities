let currentRecognition = null;

/**
 * Cancels the current speech recognition, if any.
 * @param {boolean} isAborted - Indicates whether the recognition should be aborted (true) or stopped (false).
 */
export const cancelSpeechRecognition = (isAborted) => {
   if (currentRecognition !== null) {
      if (isAborted) {
         currentRecognition.abort();
      } else {
         currentRecognition.stop();
      }
      currentRecognition = null;
   }
};

/**
 * Starts recognizing speech in the browser and registers all the callbacks.
 * @param {any} dotnetObj - The object used for callback invocations.
 * @param {string} lang - Returns and sets the language of the current SpeechRecognition.
 * If not specified, this defaults to the HTML lang attribute value, or the user agent's language setting if that isn't set either.
 * @param {boolean} continuous - Controls whether continuous results are returned for each recognition (true), or only a single result.
 * @param {boolean} interimResults - Indicates whether the recognition should return interim results.
 * @param {string} onResultMethodName - The callback invoked for incremental recognition results.
 * @param {string | null} onErrorMethodName - The optional callback invoked in the event of a recognition error.
 * @param {string | null} onStartMethodName - The optional callback invoked when recognition starts.
 * @param {string | null} onFinishMethodName - The optional callback invoked when recognition ends.
 */
export const startSpeechRecognition = (
   dotnetObj,
   lang,
   continuous,
   interimResults,
   onResultMethodName,
   onErrorMethodName,
   onStartMethodName,
   onFinishMethodName
) => {
   if (!dotnetObj || !onResultMethodName) {
      return;
   }

   let implementation = null;
   if (typeof SpeechRecognition !== 'undefined' && implementation == null) {
      implementation = SpeechRecognition;
   }
   if (typeof webkitSpeechRecognition !== 'undefined' && implementation == null) {
      implementation = webkitSpeechRecognition;
   }

   if (onErrorMethodName && implementation == null) {
      dotnetObj.invokeMethodAsync(onErrorMethodName, {
         Error: 'Not supported',
         Message: 'Speech recognition is not supported by this browser.'
      });
      return;
   }

   currentRecognition = new implementation();
   currentRecognition.continuous = continuous;
   currentRecognition.interimResults = interimResults;
   currentRecognition.lang = lang;

   if (onStartMethodName) {
      currentRecognition.onstart = () => dotnetObj.invokeMethodAsync(onStartMethodName);
   }

   if (onFinishMethodName) {
      currentRecognition.onend = () => dotnetObj.invokeMethodAsync(onFinishMethodName);
   }

   if (onErrorMethodName) {
      currentRecognition.onerror = (error) => {
         dotnetObj.invokeMethodAsync(onErrorMethodName, {
            Error: error.error,
            Message: error.message,
         });
      };
   }
   currentRecognition.onresult = (result) => {
      let transcript = '';
      let isFinal = false;
      for (let i = result.resultIndex; i < result.results.length; ++i) {
         transcript += result.results[i][0].transcript;
         if (result.results[i].isFinal) {
            isFinal = true;
         }
      }
      if (isFinal) {
         const punctuation =
            transcript.endsWith('.') ||
               transcript.endsWith('?') ||
               transcript.endsWith('!')
               ? ''
               : '.';
         transcript = `${transcript.replace(/\S/, (str) => str.toLocaleUpperCase())}${punctuation}`;
      }
      dotnetObj.invokeMethodAsync(onResultMethodName, transcript, isFinal);
   };
   currentRecognition.start();
};

// Cancel the recognition when the page is unloaded.
window.addEventListener('beforeunload', (_) => {
   cancelSpeechRecognition(true);
});

function scrollElementToTop(element) {
   if (typeof element === 'string') {
      element = document.querySelector(element);
   }

   if (element != null) {
      if (element.scrollTo) {
         element.scrollTo({ top: 0, behavior: 'smooth' });
      } else {
         element.scrollTop = 0;
      }
   }
}

function scrollElementToBottom(element) {
   if (typeof element === 'string') {
      element = document.querySelector(element);
   }

   if (element != null) {
      if (element.scrollTo) {
         element.scrollTo({ top: element.scrollHeight, behavior: 'smooth' });
      } else {
         element.scrollTop = element.scrollHeight;
      }
   }
}

window.downloadFileFromStream = async (fileName, contentStreamReference) => {
   const arrayBuffer = await contentStreamReference.arrayBuffer();
   const blob = new Blob([arrayBuffer]);
   const url = URL.createObjectURL(blob);
   const anchorElement = document.createElement('a');
   anchorElement.href = url;
   anchorElement.download = fileName ?? '';
   anchorElement.click();
   anchorElement.remove();
   URL.revokeObjectURL(url);
}


window.playAudioFileStream = async (contentStreamReference) => {

   const arrayBuffer = await contentStreamReference.arrayBuffer();
   const blob = new Blob([arrayBuffer]);
   const url = URL.createObjectURL(blob);

   var sound = document.createElement('audio');
   sound.src = url;
   sound.type = 'audio/mpeg';
   document.body.appendChild(sound);
   sound.load();
   sound.play();
   sound.onended = function () {
      document.body.removeChild(sound);
      URL.revokeObjectURL(url);
   };
}

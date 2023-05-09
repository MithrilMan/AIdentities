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

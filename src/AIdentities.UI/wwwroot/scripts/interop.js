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

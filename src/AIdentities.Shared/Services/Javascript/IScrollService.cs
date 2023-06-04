namespace AIdentities.Shared.Services.Javascript;

/// <summary>
/// Service to scroll elements into view.
/// </summary>
public interface IScrollService
{
   /// <summary>
   /// Scrolls the element to the top.
   /// </summary>
   /// <param name="element"></param>
   Task ScrollToTop(ElementReference element);

   /// <summary>
   /// Scrolls the element to the bottom.
   /// </summary>
   /// <param name="element"></param>
   Task ScrollToBottom(ElementReference element);

   /// <summary>
   /// Scrolls the element to the top of the element with the given selector.
   /// </summary>
   /// <param name="element"></param>
   Task ScrollToTop(string selector);

   /// <summary>
   /// Scrolls the element to the bottom of the element with the given selector.
   /// </summary>
   /// <param name="element"></param>
   Task ScrollToBottom(string selector);

   /// <summary>
   /// Ensures the element with the given selector is visible.
   /// </summary>
   /// <param name="selector">The selector of the element to ensure is visible.</param>
   Task EnsureIsVisible(string selector);
}

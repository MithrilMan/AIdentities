namespace AIdentities.Shared.Services.EventBus;
public interface IEventBus
{
   /// <summary>
   /// Subscribes an object to the event bus.
   /// In order to receive events, the object must implement the IHandle interface.
   /// Don't forget to unsubscribe when the object is disposed.
   /// </summary>
   /// <param name="subscriber"></param>
   public void Subscribe(object subscriber);

   /// <summary>
   /// Unsuscribe a previously subscribed object from the event bus.
   /// Remember to unsubscribe when the object is disposed.
   /// </summary>
   /// <param name="subscriber"></param>
   void Unsubscribe(object subscriber);

   /// <summary>
   /// Publishes an event on the event bus.
   /// </summary>
   /// <param name="event">The event to publish.</param>
   Task Publish(object @event);
}

namespace Microsoft.AspNetCore.Components;

public static class EventCallbackFactoryExtension
{
   public static EventCallback Create(this EventCallbackFactory factory, object receiver, Func<ValueTask> callback)
   {
      return factory.Create(receiver, () => callback().AsTask());
   }

   public static EventCallback<T> Create<T>(this EventCallbackFactory factory, object receiver, Func<T, ValueTask> callback)
   {
      return factory.Create<T>(receiver, (t) => callback(t).AsTask());
   }

   public static EventCallback<T> Create<T>(this EventCallbackFactory factory, object receiver, Func<ValueTask> callback)
   {
      return factory.Create<T>(receiver, () => callback().AsTask());
   }
}

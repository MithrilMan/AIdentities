namespace AIdentities.Shared.Utils;

public static class ThrottlerDebouncer
{
   public static Action Debounce(Action action, TimeSpan interval, CancellationToken cancellationToken)
   {
      if (action == null) throw new ArgumentNullException(nameof(action));

      var last = 0;
      return () =>
      {
         var current = Interlocked.Increment(ref last);
         Task.Delay(interval, cancellationToken).ContinueWith(task =>
         {
            if (cancellationToken.IsCancellationRequested) return;

            if (current == last)
            {
               action();
            }
         });
      };
   }

   public static Action<T> Debounce<T>(Action<T> action, TimeSpan interval, CancellationToken cancellationToken)
   {
      if (action == null) throw new ArgumentNullException(nameof(action));

      var last = 0;
      return arg =>
      {
         var current = Interlocked.Increment(ref last);
         Task.Delay(interval, cancellationToken).ContinueWith(task =>
         {
            if (cancellationToken.IsCancellationRequested) return;

            if (current == last)
            {
               action(arg);
            }
         });
      };
   }

   public static Action<T> Throttle<T>(Action<T> action, TimeSpan interval, CancellationToken cancellationToken)
   {
      if (action == null) throw new ArgumentNullException(nameof(action));

      DateTime lastExecution = DateTime.MinValue;
      Task? delayTask = null;
      var @lock = new object();
      T? args = default;

      return (arg) =>
      {
         args = arg;
         if (delayTask != null) return;

         lock (@lock)
         {
            if (delayTask != null) return;

            var timeSinceLastExecution = DateTime.UtcNow - lastExecution;
            var delayInterval = timeSinceLastExecution > interval ? TimeSpan.Zero : interval - timeSinceLastExecution;

            delayTask = Task.Delay(delayInterval, cancellationToken).ContinueWith(t =>
            {
               if (cancellationToken.IsCancellationRequested)
               {
                  return;
               }

               lastExecution = DateTime.UtcNow;
               action(args);
               delayTask = null;
            });
         }
      };
   }

   public static Action Throttle(Action action, TimeSpan interval, CancellationToken cancellationToken)
   {
      if (action == null) throw new ArgumentNullException(nameof(action));

      DateTime lastExecution = DateTime.MinValue;
      Task? delayTask = null;
      var @lock = new object();

      return () =>
      {
         if (delayTask != null) return;

         lock (@lock)
         {
            if (delayTask != null) return;

            var timeSinceLastExecution = DateTime.UtcNow - lastExecution;
            var delayInterval = timeSinceLastExecution > interval ? TimeSpan.Zero : interval - timeSinceLastExecution;

            delayTask = Task.Delay(delayInterval, cancellationToken).ContinueWith(t =>
            {
               if (cancellationToken.IsCancellationRequested)
               {
                  return;
               }

               lastExecution = DateTime.UtcNow;
               action();
               delayTask = null;
            });
         }
      };
   }
}

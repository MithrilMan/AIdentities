namespace AIdentities.Shared.Utils;

public static class Debug
{
   public static JsonSerializerOptions DebugOptions = new JsonSerializerOptions
   {
      WriteIndented = true,
      DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
   };

   public static ILogger DumpAsJson<T>(this ILogger logger, string message, T obj)
   {
      if (logger is null) throw new ArgumentNullException(nameof(logger));
      if (obj is null) throw new ArgumentNullException(nameof(obj));
      if (message is null) throw new ArgumentNullException(nameof(obj));

      logger.LogDebug("DUMP {Message}: {@Content}", message, JsonDocument.Parse(JsonSerializer.Serialize(obj, DebugOptions)));
      return logger;
   }
}

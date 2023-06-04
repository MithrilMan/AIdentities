namespace AIdentities.Shared.Features.CognitiveEngine;

public class CognitiveContext
{
   public ICognitiveEngine CognitiveEngine { get; }

   public AIdentity AIdentity => CognitiveEngine.AIdentity;

   public Dictionary<string, object?> State { get; set; } = new();

   public CognitiveContext(ICognitiveEngine cognitiveEngine)
   {
      CognitiveEngine = cognitiveEngine;
   }

   /// <summary>
   /// Get the value of the given key in the State dictionary.
   /// If the key is not present or the value is not of the given type, return the default value (cannot be null).
   /// </summary>
   /// <typeparam name="T">The type of the value to get.</typeparam>
   /// <param name="key">The key of the value to get.</param>
   /// <param name="defaultValue">The default value to return if the key is not present or the value is not of the given type.
   /// The default value cannot be null and will be stored in the dictionary.
   /// </param>
   /// <returns>The value of the given key in the State dictionary or the default (non null) value.</returns>
   /// <exception cref="ArgumentNullException"></exception>
   public T GetOrDefault<T>(string key, T defaultValue)
   {
      if (defaultValue is null) throw new ArgumentNullException(nameof(defaultValue));

      if (!State.TryGetValue(key, out var obj) || obj is not T value)
      {
         value = defaultValue;
         State[key] = value!;
      }

      return value;
   }

   /// <summary>
   /// Get the value of the given key in the State dictionary.
   /// If the key is not present or the value is not of the given type, return null.
   /// </summary>
   /// <typeparam name="T">The type of the value to get.</typeparam>
   /// <param name="key">The key of the value to get.</param>
   /// <returns>The value of the given key in the State dictionary or the default value of the type.</returns>
   public T? GetOrDefault<T>(string key)
   {
      if (!State.TryGetValue(key, out var obj) || obj is not T value)
      {
         return default;
      }

      return value;
   }

   /// <summary>
   /// Set the value of the given key in the State dictionary.
   /// if the passed value is null, the key is removed from the dictionary.
   /// </summary>
   /// <typeparam name="T">The type of the value to set.</typeparam>
   /// <param name="key">The key of the value to set.</param>
   /// <param name="value">The value to set.</param>
   public void SetOrRemove<T>(string key, T? value)
   {
      if (value is null)
      {
         State.Remove(key);
      }
      else
      {
         State[key] = value;
      }
   }
}

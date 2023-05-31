namespace AIdentities.Shared.Features.CognitiveEngine.Mission;

/// <summary>
/// Base implementation of <see cref="IMissionContext"/>.
/// Nothing fancy here, just the basic implementation of the interface.
/// You can still create more custom Mission implementationss with explicit properties 
/// mapped on the State dictionary.
/// </summary>
public class MissionContext : IMissionContext
{
   /// <inheritdoc/>
   public string Goal { get; set; } = default!;

   /// <inheritdoc/>
   public CancellationToken MissionRunningCancellationToken { get; set; }

   public AIdentitiesConstraint AIdentitiesConstraints { get; init; } = new();

   /// <inheritdoc/>
   public ResourceConstraints ResourceConstraints { get; init; } = new();

   /// <inheritdoc/>
   public List<SkillConstraint> SkillConstraints { get; init; } = new();

   /// <inheritdoc/>
   public Dictionary<string, object> State { get; init; } = new();

   /// <inheritdoc/>
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

   /// <inheritdoc/>
   public T? GetOrDefault<T>(string key)
   {
      if (!State.TryGetValue(key, out var obj) || obj is not T value)
      {
         return default;
      }

      return value;
   }

   /// <inheritdoc/>
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

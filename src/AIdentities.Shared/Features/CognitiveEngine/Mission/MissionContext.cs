using System.Diagnostics.CodeAnalysis;

namespace AIdentities.Shared.Features.CognitiveEngine.Mission;

/// <summary>
/// Default implementation of a mission context.
/// You can still create more custom Mission implementationss with explicit properties 
/// mapped on the State dictionary.
/// </summary>
public class MissionContext //: IMissionContext
{
   /// <summary>
   /// The mission goal.
   /// </summary>
   public string Goal { get; set; } = default!;

   /// <summary>
   /// This token is valid while the mission is running.
   /// When the mission is stopped, this token is cancelled.
   /// This token can be used to stop the execution of async operations that are part of the mission.
   /// </summary>
   public CancellationToken MissionRunningCancellationToken { get; set; }

   /// <summary>
   /// The AIdentities constraints of the mission.
   /// </summary>
   public AIdentitiesConstraint AIdentitiesConstraints { get; init; } = new();

   /// <summary>
   /// The resource constraints of the mission.
   /// </summary>
   public ResourceConstraints ResourceConstraints { get; init; } = new();

   /// <summary>
   /// The skill constraints of the mission.
   /// Which skills are available in the mission and (optionally) which AIdentity is preferred for each skill.
   /// Only the skill included in this list should be used to accomplish the mission tasks.
   /// This applies only for skills that are supposed to be automatically triggered by other skills or during
   /// the automatic flow of a mission, but a specific skill can still be triggered programmatically.
   /// </summary>
   public List<SkillConstraint> SkillConstraints { get; init; } = new();

   /// <summary>
   /// Hold any state information that can be used by any skill and AIdentity in the mission.
   /// A sort of collective meaningfull memory.
   /// </summary>
   protected Dictionary<string, object> State { get; init; } = new();

   /// <summary>
   /// Get the value of the given key in the State dictionary.
   /// If the key is not present or the value is not of the given type, return the default value (cannot be null).
   /// </summary>
   /// <typeparam name="T">The type of the value to get.</typeparam>
   /// <param name="key">The key of the value to get.</param>
   /// <param name="defaultValue">The default value to return if the key is not present or the value is not of the given type.
   /// The default value cannot be null and will be stored in the dictionary.
   /// </param>
   /// <returns>The value of the given key in the State dictionary or the default value.</returns>
   [return: NotNullIfNotNull(nameof(defaultValue))]
   public T? GetOrDefault<T>(string key, T? defaultValue)
   {
      if (State.TryGetValue(key, out var obj) && obj is T value) return value;

      if (defaultValue != null)
      {
         State[key] = defaultValue;
      }

      return defaultValue;
   }

   /// <summary>
   /// Get the value of the given key in the State dictionary.
   /// If the key is not present or the value is not of the given type, return null.
   /// </summary>
   /// <typeparam name="T">The type of the value to get.</typeparam>
   /// <param name="key">The key of the value to get.</param>
   /// <returns>The value of the given key in the State dictionary or the default value of the type.</returns>
   public bool TryGet<T>(string key, [MaybeNullWhen(false)] out T value)
   {
      if (!State.TryGetValue(key, out var obj) || obj is not T extractedValue)
      {
         value = default;
         return false;
      }

      value = extractedValue;
      return true;
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

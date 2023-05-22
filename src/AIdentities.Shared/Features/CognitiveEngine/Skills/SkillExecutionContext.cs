using System.Collections.Concurrent;
using AIdentities.Shared.Features.CognitiveEngine.Mission;

namespace AIdentities.Shared.Features.CognitiveEngine.Skills;
public class SkillExecutionContext
{
   public CognitiveContext CognitiveContext { get; }

   public MissionContext? MissionContext { get; }

   readonly ConcurrentDictionary<string, object?> _state = new();

   public SkillExecutionContext(CognitiveContext cognitiveContext, MissionContext? missionContext)
   {
      CognitiveContext = cognitiveContext;
      MissionContext = missionContext;
   }


   public T? GetInput<T>(string key, SkillVariableType type) where T : class
      => GetVariable<T>($"OUT_{key}", type);
   public void SetInput(string key, SkillVariableType type, object? value)
      => SetVariable($"IN_{key}", type, value);

   public T? GetOutput<T>(string key, SkillVariableType type) where T : class
      => GetVariable<T>($"OUT_{key}", type);
   public void SetOutput(string key, SkillVariableType type, object? value)
      => SetVariable($"OUT_{key}", type, value);

   protected void SetVariable(string key, SkillVariableType type, object? value)
   {
      if (value is null)
      {
         _state.TryRemove(key, out _);
      }
      else
      {
         _state[key] = type switch
         {
            SkillVariableType.Unknown => throw new NotImplementedException(),
            SkillVariableType.String => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture),
            SkillVariableType.Integer => Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture),
            SkillVariableType.Double => Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture),
            SkillVariableType.Boolean => Convert.ToBoolean(value, System.Globalization.CultureInfo.InvariantCulture),
            SkillVariableType.Epoch => Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture),
            SkillVariableType.TimeSpan => TimeSpan.FromMilliseconds(Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture)),
            SkillVariableType.Base64 => Convert.ToBase64String((byte[])value),
            SkillVariableType.Guid => Guid.Parse(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)),
            _ => throw new NotImplementedException(),
         };
      }
   }

   protected T? GetVariable<T>(string key, SkillVariableType type) where T : class
   {
      if (_state.TryGetValue($"IN_{key}", out var value))
      {
         return type switch
         {
            SkillVariableType.Unknown => throw new NotImplementedException(),
            SkillVariableType.String => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) as T,
            SkillVariableType.Integer => Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture) as T,
            SkillVariableType.Double => Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture) as T,
            SkillVariableType.Boolean => Convert.ToBoolean(value, System.Globalization.CultureInfo.InvariantCulture) as T,
            SkillVariableType.Epoch => Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture) as T,
            SkillVariableType.TimeSpan => TimeSpan.FromMilliseconds(Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture)) as T,
            SkillVariableType.Base64 => Convert.FromBase64String(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)) as T,
            SkillVariableType.Guid => Guid.Parse(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)) as T,
            _ => throw new NotImplementedException(),
         };
      }
      else
      {
         return default!;
      }
   }
}

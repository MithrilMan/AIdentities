using System.Collections.Concurrent;
using AIdentities.Shared.Features.CognitiveEngine.Mission;
using AIdentities.Shared.Features.CognitiveEngine.Prompts;
using AIdentities.Shared.Features.CognitiveEngine.Thoughts;

namespace AIdentities.Shared.Features.CognitiveEngine.Skills;
public class SkillExecutionContext
{
   public CognitiveContext CognitiveContext { get; }

   public MissionContext? MissionContext { get; }

   /// <summary>
   /// The AIdentity that is executing the skill.
   /// It's the owner of CognitiveContext.
   /// </summary>
   public AIdentity AIdentity => CognitiveContext.AIdentity;

   /// <summary>
   /// The current executing skill.
   /// This is set by the cognitive engine before invoking the skill.
   /// </summary>
   public SkillDefinition CurrentSkill { get; internal set; }

   /// <summary>
   /// A stack of prompt that are stacked at each execution of a new action.
   /// </summary>
   public Stack<Prompt> PromptChain { get; } = new();

   readonly ConcurrentDictionary<string, object?> _state = new();

   public SkillExecutionContext(ISkill skill, CognitiveContext cognitiveContext, MissionContext? missionContext)
   {
      CurrentSkill = skill.Definition;
      CognitiveContext = cognitiveContext;
      MissionContext = missionContext;
   }

   /// <summary>
   /// Generates an <see cref="InvalidPromptResponseThought"/>.
   /// This is a special type of <see cref="IntrospectiveThought"/> that is used when the
   /// cognitive engine is not able to handle the conversion the AIdentity response to a prompt
   /// into a valid value for the skill action.
   /// </summary>
   /// <param name="skill">The optional skill action that is being executed.</param>
   /// <param name="howToFix">The explaination about how to fix the invalid response, or the error description.</param>
   /// <returns>The invalid prompt thought.</returns>
   public InvalidArgumentsThought InvalidArgumentsThought(string howToFix)
      => new(CurrentSkill.Name, AIdentity, howToFix);

   /// <summary>
   /// Generates an <see cref="InvalidPromptResponseThought"/>.
   /// This is a special type of <see cref="IntrospectiveThought"/> that is used when the 
   /// cognitive engine is not able to understand the user's response to a prompt.
   /// In this specific case, the caller should try to provide missing arguments before invoking again the action.
   /// </summary>
   /// <param name="skill">The optional skill action that is being executed.</param>
   /// <param name="missingArguments">The list of missing arguments.</param>
   /// <returns>The invalid prompt response thought.</returns>
   public MissingArgumentsThought MissingArgumentsThought(params string[] missingArguments)
      => new MissingArgumentsThought(CurrentSkill.Name, AIdentity, missingArguments);

   public IntrospectiveThought IntrospectiveThought(string thought)
      => new IntrospectiveThought(CurrentSkill.Name, AIdentity, thought);

   /// <summary>
   /// Produces an <see cref="ActionThought"/>.
   /// Consider this as a way to log what the cognitive engine is doing.
   /// </summary>
   /// <param name="skill">The optional skill action that is being executed.</param>
   /// <param name="thought">The thought to produce.</param>
   /// <returns>The action thought.</returns>
   public ActionThought ActionThought(string thought)
      => new ActionThought(CurrentSkill.Name, AIdentity, thought);

   /// <summary>
   /// Generates a <see cref="FinalThought"/>.
   /// A final thought is something that the cognitive engine can return to the user.
   /// </summary>
   /// <param name="skill">The optional skill action that is being executed.</param>
   /// <param name="thought">The thought to produce.</param>
   /// <returns>The final thought.</returns>
   public FinalThought FinalThought(string thought)
      => new FinalThought(CurrentSkill.Name, AIdentity, thought);

   /// <summary>
   /// Generates a <see cref="StreamedThought"/>.
   /// A streamed thought is supposed to be updated until it generates the last chunk of information.
   /// At that point StreamedThought.IsStreamComplete should be set to true.
   /// </summary>
   /// <param name="skill">The optional skill action that is being executed.</param>
   /// <param name="thought">The thought to produce.</param>
   /// <returns>The first streamed thought.</returns>
   public StreamedFinalThought StreamFinalThought(string thought)
      => new StreamedFinalThought(CurrentSkill.Name, AIdentity, thought);

   /// <summary>
   /// Generates a <see cref="StreamedThought"/>.
   /// A streamed thought is supposed to be updated until it generates the last chunk of information.
   /// At that point StreamedThought.IsStreamComplete should be set to true.
   /// </summary>
   /// <param name="skill">The optional skill action that is being executed.</param>
   /// <param name="thought">The thought to produce.</param>
   /// <returns>The first streamed thought.</returns>
   public StreamedIntrospectiveThought StreamIntrospectiveThought(string thought)
      => new StreamedIntrospectiveThought(CurrentSkill.Name, AIdentity, thought);


   public T? GetInput<T>(string key, SkillVariableType type) where T : class => GetVariable<T>(key, type);
   public void SetInput(string key, SkillVariableType type, object? value) => SetVariable(key, type, value);
   public T? GetInput<T>(string key) where T : class => GetVariable<T>(key, GetInputVariableType(key));
   public void SetInput(string key, object? value) => SetVariable(key, GetInputVariableType(key), value);

   public T? GetOutput<T>(string key, SkillVariableType type) where T : class => GetVariable<T>(key, type);
   public void SetOutput(string key, SkillVariableType type, object? value) => SetVariable(key, type, value);
   public T? GetOutput<T>(string key) where T : class => GetVariable<T>(key, GetOutputVariableType(key));
   public void SetOutput(string key, object? value) => SetVariable(key, GetOutputVariableType(key), value);

   protected T? GetVariable<T>(string key, SkillVariableType type) where T : class
   {
      if (_state.TryGetValue(key, out var value))
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
            SkillVariableType.Base64 => Convert.FromBase64String(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? "") as T,
            _ => throw new NotImplementedException(),
         };
      }
      else
      {
         return default!;
      }
   }

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
            _ => throw new NotImplementedException(),
         };
      }
   }

   private SkillVariableType GetInputVariableType(string key)
      => CurrentSkill.Inputs.FirstOrDefault(i => i.Name == key)?.Type
      ?? throw new ArgumentException($"Input {key} not found in skill {CurrentSkill.Name}");

   private SkillVariableType GetOutputVariableType(string key)
      => CurrentSkill.Outputs.FirstOrDefault(i => i.Name == key)?.Type
      ?? throw new ArgumentException($"Output {key} not found in skill {CurrentSkill.Name}");
}

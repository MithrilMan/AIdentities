using System.Diagnostics.CodeAnalysis;
using AIdentities.Shared.Features.CognitiveEngine.Mission;
using AIdentities.Shared.Features.CognitiveEngine.Prompts;
using AIdentities.Shared.Features.CognitiveEngine.Thoughts;

namespace AIdentities.Shared.Features.CognitiveEngine.Skills;

public abstract class Skill : ISkill
{
   public Guid Id { get; } = Guid.NewGuid();
   public string Name { get; }

   public Skill(string name)
   {
      Name = name;
   }

   public abstract IAsyncEnumerable<Thought> ExecuteAsync(Prompt prompt, SkillExecutionContext executionContext, CancellationToken cancellationToken);

   /// <summary>
   /// Extracts the typeod arguments from the text.
   /// </summary>
   /// <typeparam name="TReturnValue">The type of the arguments to extract.</typeparam>
   /// <param name="text">The text to extract the arguments from.</param>
   /// <returns>The extracted arguments.</returns>
   public virtual bool TryExtractJson<TReturnValue>(string text, [MaybeNullWhen(false)] out TReturnValue args) where TReturnValue : class
   {
      var json = SkillRegexUtils.ExtractJson().Match(text).Value;
      try
      {
         args = JsonSerializer.Deserialize<TReturnValue>(json)!;
         return true;
      }
      catch (Exception)
      {
         args = default;
         return false;
      }
   }

   /// <summary>
   /// Try to extract the arguments from the contexts.
   /// First it will try to extract the arguments from the mission context.
   /// If it fails, it will try to extract the arguments from the cognitive context.
   /// </summary>
   /// <typeparam name="TReturnValue">The type of the arguments to extract.</typeparam>
   /// <param name="cognitiveContext">The cognitive context.</param>
   /// <param name="missionContext">The mission context.</param>
   /// <param name="args">The extracted arguments.</param>
   /// <returns>The extracted arguments.</returns>
   public virtual bool TryExtractFromContext<TReturnValue>(
      CognitiveContext cognitiveContext,
      MissionContext? missionContext,
      [MaybeNullWhen(false)] out TReturnValue args) where TReturnValue : class
   {

      var mission_skill_key = $"{cognitiveContext.AIdentity}_{Name}_{Id}";
      if (cognitiveContext.State.TryGetValue(mission_skill_key, out object? rawValue)
         && rawValue is TReturnValue missionContextValue)
      {
         args = missionContextValue;
         return true;
      }

      var cognitive_skill_key = $"{Name}_{Id}";
      if (cognitiveContext.State.TryGetValue(cognitive_skill_key, out rawValue)
         && rawValue is TReturnValue cognitiveContextValue)
      {
         args = cognitiveContextValue;
         return true;
      }

      args = null;
      return false;
   }

   /// <summary>
   /// Tries to extract the json from the text.
   /// </summary>
   /// <param name="text"></param>
   /// <param name="json"></param>
   /// <returns></returns>
   public virtual bool TryExtractJson(string text, [MaybeNullWhen(false)] out string json)
   {
      json = SkillRegexUtils.ExtractJson().Match(text).Value;
      return !string.IsNullOrWhiteSpace(json);
   }
}

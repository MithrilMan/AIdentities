using System.Diagnostics.CodeAnalysis;
using AIdentities.Shared.Features.CognitiveEngine.Prompts;
using AIdentities.Shared.Features.CognitiveEngine.Thoughts;

namespace AIdentities.Shared.Features.CognitiveEngine.Skills;

public abstract class Skill : ISkill
{
   SkillDefinition ISkill.Definition { get; set; } = default!;

   public SkillDefinition Definition => ((ISkill)this).Definition;
   public string Name => ((ISkill)this).Definition.Name;

   public async IAsyncEnumerable<Thought> ExecuteAsync(
      Prompt prompt,
      SkillExecutionContext context,
      [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      context.PromptChain.Push(prompt);

      if (!ValidateInputs(prompt, context, out var howToRemedy))
      {
         yield return howToRemedy;
      }

      var thoughts = ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
      await foreach (var thought in thoughts)
      {
         yield return thought;
      }
   }

   /// <summary>
   /// Check if the inputs are valid.
   /// If there is something it can do to fix the inputs, it should do it.
   /// </summary>
   /// <param name="error">
   /// An explaination about how to remedy to the failed validation inputs.
   /// This text could be used to let the LLM try to fix it, or to let the user know what is wrong.
   /// </param>
   /// <returns>True if the inputs are valid, false otherwise.</returns>
   protected abstract bool ValidateInputs(Prompt prompt, SkillExecutionContext context, [MaybeNullWhen(true)] out InvalidArgumentsThought error);

   /// <summary>
   /// Execute the skill.
   /// Skillcontext contains the current state of the execution 
   /// and a chain of Prompts that has been used to reach this point.
   /// It contains also all the input and output variables set so far.
   /// </summary>
   /// <param name="context">The execution context.</param>
   /// <param name="cancellationToken">The cancellation token.</param>
   /// <returns></returns>
   protected abstract IAsyncEnumerable<Thought> ExecuteAsync(SkillExecutionContext context, CancellationToken cancellationToken);

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
      SkillExecutionContext context,
      [MaybeNullWhen(false)] out TReturnValue args) where TReturnValue : class
   {
      var cognitiveContext = context.CognitiveContext;
      var mission_skill_key = $"{cognitiveContext.AIdentity}_{Name}";
      if (cognitiveContext.State.TryGetValue(mission_skill_key, out object? rawValue)
         && rawValue is TReturnValue missionContextValue)
      {
         args = missionContextValue;
         return true;
      }

      var cognitive_skill_key = $"{Name}";
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

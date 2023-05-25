using AIdentities.Shared.Features.CognitiveEngine.Mission;
using AIdentities.Shared.Features.CognitiveEngine.Prompts;
using AIdentities.Shared.Features.CognitiveEngine.Thoughts;
using AIdentities.Shared.Plugins.Connectors.Completion;
using AIdentities.Shared.Plugins.Connectors.Conversational;

namespace AIdentities.Shared.Features.CognitiveEngine.Engines.Reflexive;

/// <summary>
/// This cognitive engine is an abstract cognitive engine that tries to reflect upon itself to perform any task it can.
/// When a prompt is given to it, it tries to find the best skill to accomplish it.
/// If it can't find a skill, it calls the <see cref="HandleNoCommandDetected(Prompt, IMissionContext?, CancellationToken)"/> method.
/// If a skill is found, it executes it and waits for the result of such skill.
/// If the detected skill is unknown (sometimes LLM can hallucinate commands instructions we give it), it calls the
/// <see cref="HandleUnknownCommandDetected(Prompt, string, IMissionContext?, CancellationToken)"/> method, this may give the chance
/// to reissue the prompt to re-evaluate it, or to perform a different action.
/// If the skill has trouble executing, it tries to find a conversation piece to reply to the user.
/// </summary>
public abstract class ReflexiveCognitiveEngine<TCognitiveContext> : CognitiveEngine<TCognitiveContext>
   where TCognitiveContext : CognitiveContext
{
   protected const string UNKNOWN_SKILL = PromptTemplates.UNKNOWN_SKILL;

   public bool IsFirstPrompt { get; set; } = true;

   public ReflexiveCognitiveEngine(ILogger<ReflexiveCognitiveEngine<TCognitiveContext>> logger,
                                 AIdentity aIdentity,
                                 IConversationalConnector defaultConversationalConnector,
                                 ICompletionConnector defaultCompletionConnector,
                                 ISkillManager skillManager)
      : base(logger, aIdentity, defaultConversationalConnector, defaultCompletionConnector, skillManager) { }


   public override async IAsyncEnumerable<Thought> HandlePromptAsync(Prompt prompt, IMissionContext? missionContext, [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      if (IsFirstPrompt)
      {
         IsFirstPrompt = false;
         bool lastThoughtWasFinal = false;
         await foreach (var thought in HandleFirstPrompt(prompt, missionContext, cancellationToken).ConfigureAwait(false))
         {
            lastThoughtWasFinal = thought is FinalThought;
            yield return thought;
         }
         if (lastThoughtWasFinal) yield break;
      }


      // try to detect a skill that can handle the prompt, excluding the skills that are disabled
      bool missionHasSkillConstraints = missionContext is { SkillConstraints.Count: > 0 };
      var availableSkills = missionHasSkillConstraints ? missionContext!.SkillConstraints.Select(s => s.Skill) : EnabledSkills;

      // if no skills are available, bypass the detection and go straight to the no command detected handler
      if (!availableSkills.Any())
      {
         await foreach (var thought in HandleNoCommandDetected(prompt, missionContext, cancellationToken).ConfigureAwait(false))
         {
            yield return thought;
         }
         yield break;
      }


      (bool skillDetected, string detectedSkill, string? jsonArgs)
         = await TryDetectSkillAsync(prompt, availableSkills, cancellationToken).ConfigureAwait(false);

      if (!skillDetected || detectedSkill == UNKNOWN_SKILL)
      {
         await foreach (var thought in HandleNoCommandDetected(prompt, missionContext, cancellationToken).ConfigureAwait(false))
         {
            yield return thought;
         }
         yield break;
      }

      yield return ActionThought($"I detected the skill {detectedSkill}.");
      if (_skillManager.Get(detectedSkill) is { } skill)
      {
         var skillExecutionContext = new SkillExecutionContext(skill, Context, missionContext);

         //jsonArgs may contain the arguments to pass to the skill
         //try to apply them to the skillContext using the SkillDefinition of the skill
         if (!string.IsNullOrEmpty(jsonArgs))
         {
            try
            {
               var args = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonArgs);
               if (args != null)
               {
                  foreach (var arg in args)
                  {
                     skillExecutionContext.SetInput(arg.Key, arg.Value);
                  }
               }
            }
            catch (Exception)
            {
               ActionThought($"I detected the skill {detectedSkill} but I couldn't deserialize the arguments.");
            }
         }

         var thoughtStream = skill.ExecuteAsync(prompt, skillExecutionContext, cancellationToken).ConfigureAwait(false);
         await foreach (var thought in thoughtStream)
         {
            yield return thought;
         }
      }
      else
      {
         await foreach (var thought in HandleUnknownCommandDetected(prompt, detectedSkill, missionContext, cancellationToken).ConfigureAwait(false))
         {
            yield return thought;
         }
      }

      yield break;
   }


   /// <summary>
   /// Handles the first prompt received by the cognitive engine.
   /// When it ends it will keep the standard flow and will try to detect a skill in the prompt.
   /// Override this method to perform a custom action when the first prompt is received.
   /// If you want the first action to stop the standard flow, return a FinalThought as a last thought.
   /// Note that if HandlePromptAsync is overridden, this method may behave differently or may
   /// not be called at all.
   /// </summary>
   /// <param name="prompt">The first prompt.</param>
   /// <param name="missionContext">The mission context, if any.</param>
   /// <param name="cancellationToken">A cancellation token.</param>
   /// <returns>
   /// A stream of thoughts. If a FinalThought is returned, the standard flow will be stopped except
   /// an override of HandlePromptAsync of the default ReflexiveCognitiveEngine is implemented.
   /// </returns>
   protected virtual async IAsyncEnumerable<Thought> HandleFirstPrompt(
      Prompt prompt,
      IMissionContext? missionContext,
      [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      await Task.CompletedTask.ConfigureAwait(false);
      yield return ActionThought($"Here I am, {(missionContext is null ? "on my duty" : $"my mission is: {missionContext.Goal}")}");
   }

   /// <summary>
   /// Handles the case where an unknown command was detected in the prompt.
   /// </summary>
   /// <param name="prompt">The original prompt.</param>
   /// <param name="detectedSkillName">The name of the detected unknown skill.</param>
   /// <param name="missionContext">The mission context.</param>
   /// <param name="cancellationToken">A cancellation token.</param>
   /// <returns>A stream of thoughts.</returns>
   protected virtual async IAsyncEnumerable<Thought> HandleUnknownCommandDetected(
      Prompt prompt,
      string detectedSkillName,
      IMissionContext? missionContext,
      [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      await Task.CompletedTask.ConfigureAwait(false);
      yield return ActionThought($"I don't know the skill {detectedSkillName}");
   }

   /// <summary>
   /// Handles the case where no command was detected in the prompt.
   /// </summary>
   /// <param name="prompt">The original prompt.</param>
   /// <param name="missionContext">The mission context.</param>
   /// <param name="cancellationToken">A cancellation token.</param>
   /// <returns>A stream of thoughts.</returns>
   protected virtual async IAsyncEnumerable<Thought> HandleNoCommandDetected(
      Prompt prompt,
      IMissionContext? missionContext,
      [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      await Task.CompletedTask.ConfigureAwait(false);
      yield return ActionThought("I don't know what to do.");
   }

   /// <summary>
   /// Tries to detect a skill in the prompt.
   /// The result is stored in the CognitiveContext "detected_skill" property.
   /// </summary>
   /// <param name="prompt">The prompt to analyze.</param>
   /// <param name="availableSkills">The available skills.</param>
   /// <param name="cancellationToken">A cancellation token.</param>
   /// <returns>A tuple with the first item being a boolean indicating if a skill was detected and the second item being the detected skill name.</returns>
   protected async Task<(bool skillDetected, string detectedSkill, string? jsonArgs)> TryDetectSkillAsync(
      Prompt prompt,
      IEnumerable<SkillDefinition> availableSkills,
      CancellationToken cancellationToken)
   {
      var instruction = PromptTemplates.BuildFindSkillPrompt(prompt, availableSkills);
      var response = await _defaultCompletionConnector.RequestCompletionAsync(new DefaultCompletionRequest
      {
         Prompt = instruction,
         MaxGeneratedTokens = 250,
         Temperature = 0,
      }, cancellationToken).ConfigureAwait(false);

      var detectedSkill = SkillRegexUtils.ExtractSkillName().Match(response!.GeneratedMessage!).Value;
      bool skillDetected = !string.IsNullOrEmpty(detectedSkill);


      string? jsonArgs = null;
      if (skillDetected && detectedSkill != UNKNOWN_SKILL)
      {
         var skill = _skillManager.GetSkillDefinition(detectedSkill);
         if (skill is not null)
         {
            //if the skill has input arguments, try to fetch them from the prompt
            if (skill.Inputs.Count > 0)
            {
               // try to detect arguments out of the prompt
               instruction = PromptTemplates.BuildGenerateSkillParametersJson(prompt, skill);
               response = await _defaultCompletionConnector.RequestCompletionAsync(new DefaultCompletionRequest
               {
                  Prompt = instruction,
                  MaxGeneratedTokens = 500, //TODO: make this configurable
                  Temperature = 0,
               }, cancellationToken).ConfigureAwait(false);

               jsonArgs = SkillRegexUtils.ExtractJson().Match(response!.GeneratedMessage!).Value;
            }
         }
      }

      return (
         skillDetected,
         detectedSkill,
         jsonArgs
         );
   }
}

using System.Runtime.CompilerServices;
using AIdentities.Shared.Features.CognitiveEngine.Mission;
using AIdentities.Shared.Features.CognitiveEngine.Prompts;
using AIdentities.Shared.Features.CognitiveEngine.Skills;
using AIdentities.Shared.Features.CognitiveEngine.Thoughts;
using AIdentities.Shared.Plugins.Connectors.Completion;
using AIdentities.Shared.Plugins.Connectors.Conversational;

namespace AIdentities.Shared.Features.CognitiveEngine.Engines.Mithril;

/// <summary>
/// This cognitive engine tries to mimic the human brain.
/// When a task is assigned to it, it tries to find the best skill to accomplish it.
/// If it can't find a skill, it interprets the prompt as a conversation piece and tries to find a reply.
/// If a skill is found, it executes it and waits for the result of such skill.
/// If the skill has trouble executing, it tries to find a conversation piece to reply to the user.
/// The model keep in memory the conversation pieces in its CognitiveContext in order to be able to iterate with the user about the same topic.
/// Skills can access to this memory to perform their tasks.
/// </summary>
public class MithrilCognitiveEngine : CognitiveEngine<MithrilCognitiveContext>
{
   protected const string UNKNOWN_SKILL = "DUNNO";

   public bool IsFirstPrompt { get; set; } = true;

   public bool IsWaitingUserFeedback { get; set; }

   /// <summary>
   /// Holds the skills that are waiting for a user prompt.
   /// Whenever a skill action required from this AIdentity requires a user prompt, it is added to this dictionary.
   /// </summary>
   private readonly Dictionary<Guid, ISkill> _skillsWaitingPrompt = new();

   public MithrilCognitiveEngine(ILogger<MithrilCognitiveEngine> logger,
                                 AIdentity aIdentity,
                                 IConversationalConnector defaultConversationalConnector,
                                 ICompletionConnector defaultCompletionConnector,
                                 ISkillManager skillActionsManager)
      : base(logger, aIdentity, defaultConversationalConnector, defaultCompletionConnector, skillActionsManager)
   {
   }

   public override MithrilCognitiveContext CreateCognitiveContext() => new MithrilCognitiveContext(AIdentity);

   public override async IAsyncEnumerable<Thought> HandlePromptAsync(Prompt prompt, IMissionContext? missionContext, [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      if (IsFirstPrompt)
      {
         IsFirstPrompt = false;
         yield return ActionThought($"Here I am, {(missionContext is null ? "on my duty" : $"my mission is: {missionContext.Goal}")}");
      }

      (bool skillDetected, string detectedSkill, string? jsonArgs)
         = await TryDetectSkillAsync(prompt, cancellationToken).ConfigureAwait(false);

      if (skillDetected && detectedSkill != UNKNOWN_SKILL)
      {
         yield return ActionThought($"I detected the skill {detectedSkill}.");
         if (_skillManager.Get(detectedSkill) is { } skill)
         {
            //_skillsWaitingPrompt.Add(skillAction.Id, skillAction);

            var skillExecutionContext = new SkillExecutionContext(skill, Context, missionContext);

            //jsonArgs may contain the arguments to pass to the skill
            //try to apply them to the skillContext using the SkillDefinition of the skill
            if (jsonArgs is not null)
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

            var thoughtStream = skill.ExecuteAsync(prompt, skillExecutionContext, cancellationToken).ConfigureAwait(false);
            await foreach (var thought in thoughtStream)
            {
               yield return thought;
            }
         }
         else
         {
            yield return ActionThought($"I don't know the skill {detectedSkill}");
         }

         yield break;
      }

      await foreach (var thought in HandleNoCommandDetected(prompt, missionContext, cancellationToken).ConfigureAwait(false))
      {
         yield return thought;
      }
   }

   private virtual async IAsyncEnumerable<Thought> HandleNoCommandDetected(Prompt prompt,
                                                                     IMissionContext? missionContext,
                                                                     [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      //TODO: default behavior when no command has been found.
   }

   /// <summary>
   /// Tries to detect a skill in the prompt.
   /// The result is stored in the CognitiveContext "detected_skill" property.
   /// </summary>
   /// <param name="prompt">The prompt to analyze.</param>
   /// <param name="cancellationToken">A cancellation token.</param>
   /// <returns>A tuple with the first item being a boolean indicating if a skill was detected and the second item being the detected skill name.</returns>
   protected async Task<(bool skillDetected, string detectedSkill, string? jsonArgs)> TryDetectSkillAsync(Prompt prompt, CancellationToken cancellationToken)
   {
      var instruction = PromptTemplates.BuildFindSkillPrompt(prompt, _skillManager.GetSkillDefinitions());
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

      return (
         skillDetected: skillDetected,
         detectedSkill: detectedSkill,
         jsonArgs: jsonArgs
         );
   }


   private ActionThought ActionThought(string content) => new ActionThought(null, AIdentity, content);
}

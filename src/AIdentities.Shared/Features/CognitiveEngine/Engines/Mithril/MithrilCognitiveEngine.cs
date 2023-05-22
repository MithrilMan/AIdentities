using System.Runtime.CompilerServices;
using AIdentities.Shared.Features.CognitiveEngine.Mission;
using AIdentities.Shared.Features.CognitiveEngine.Prompts;
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
   public bool IsFirstPrompt { get; set; } = true;

   public bool IsWaitingUserFeedback { get; set; }

   private const string UNKNOWN_SKILL = "DUNNO";

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

   public override async IAsyncEnumerable<Thought> HandlePromptAsync(Prompt prompt, MissionContext? missionContext, [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      bool isFirstPrompt = IsFirstPrompt;
      if (IsFirstPrompt)
      {
         IsFirstPrompt = false;
         yield return Context.ActionThought(null, $"Here I am, {(missionContext is null ? "on my duty" : "working for a mission!")}");
      }

      var thoughts = (prompt switch
      {
         UserPrompt userPrompt => HandleUserPrompt(userPrompt, missionContext, isFirstPrompt, cancellationToken),
         SKillResultPrompt skillResultPrompt => HandleSKillResultPrompt(skillResultPrompt, missionContext, isFirstPrompt, cancellationToken),
         ThoughtResultPrompt thoughtResultPrompt => HandleThoughtResultPrompt(thoughtResultPrompt, missionContext, isFirstPrompt, cancellationToken),
         _ => throw new NotImplementedException(),
      }).ConfigureAwait(false);

      await foreach (var thought in thoughts)
      {
         yield return thought;
      }
   }

   protected async IAsyncEnumerable<Thought> HandleUserPrompt(
      UserPrompt prompt,
      MissionContext? missionContext,
      bool isFirstPrompt,
      [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      (bool skillDetected, string detectedSkill, string? jsonArgs)
         = await TryDetectSkillAsync(prompt, cancellationToken).ConfigureAwait(false);

      if (skillDetected && detectedSkill != UNKNOWN_SKILL)
      {
         yield return Context.ActionThought(null, $"I detected the skill {detectedSkill}.");
         var skillAction = _skillManager.Get(detectedSkill);
         if (skillAction is not null)
         {
            //_skillsWaitingPrompt.Add(skillAction.Id, skillAction);
            Context.SetSkillJsonArgs(skillAction.Id, jsonArgs);
            var thoughtStream = skillAction.ExecuteAsync(prompt, Context, missionContext, cancellationToken).ConfigureAwait(false);
            await foreach (var thought in thoughtStream)
            {
               yield return thought;
            }
         }
      }
   }

   protected IAsyncEnumerable<Thought> HandleThoughtResultPrompt(ThoughtResultPrompt thoughtResultPrompt, MissionContext? missionContext, bool isFirstPrompt, CancellationToken cancellationToken)
   {
      throw new NotImplementedException();
   }

   protected IAsyncEnumerable<Thought> HandleSKillResultPrompt(SKillResultPrompt skillResultPrompt, MissionContext? missionContext, bool isFirstPrompt, CancellationToken cancellationToken)
   {
      throw new NotImplementedException();
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
      var instruction = PromptTemplates.BuildFindSkillPrompt(prompt, _skillManager.All());
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
         var skill = _skillManager.Get(detectedSkill);
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
}

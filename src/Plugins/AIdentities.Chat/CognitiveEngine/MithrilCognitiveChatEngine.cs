using System.Runtime.CompilerServices;
using AIdentities.Shared.Features.CognitiveEngine.Engines.Mithril;
using AIdentities.Shared.Plugins.Connectors.Completion;

namespace AIdentities.Chat.CognitiveEngine;

/// <summary>
/// This cognitive engine replies to every UserPrompt it receives.
/// </summary>
public class MithrilCognitiveChatEngine : MithrilCognitiveEngine
{
   public MithrilCognitiveChatEngine(ILogger<MithrilCognitiveChatEngine> logger,
                                 AIdentity aIdentity,
                                 IConversationalConnector defaultConversationalConnector,
                                 ICompletionConnector defaultCompletionConnector,
                                 ISkillManager skillActionsManager)
      : base(logger, aIdentity, defaultConversationalConnector, defaultCompletionConnector, skillActionsManager)
   {
   }

   public override async IAsyncEnumerable<Thought> HandlePromptAsync(Prompt prompt, IMissionContext? missionContext, [EnumeratorCancellation] CancellationToken cancellationToken)
   {
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

      //
   }
}

using System.Diagnostics.CodeAnalysis;
using AIdentities.Chat.Skills.ReplyToPrompt;
using AIdentities.Shared.Plugins.Connectors.Completion;

namespace AIdentities.Shared.Features.CognitiveEngine.Engines.Conversational;

/// <summary>
/// This conversational cognitive engine handles the prompts in a conversational way, using
/// all the times the conversational connector without asking itself if she has to execute any skill.
/// The way to make AIdenitities execute skills in chat mode is by making the Chat Keeper to trigger
/// them on AIdentity's behalf.
/// </summary>
public class ChatCognitiveEngine : CognitiveEngine<CognitiveContext>
{
   readonly ISkill? _replySkill;

   public ChatCognitiveEngine(ILogger logger,
                                        AIdentity aIdentity,
                                        IConversationalConnector defaultConversationalConnector,
                                        ICompletionConnector defaultCompletionConnector,
                                        ISkillManager skillManager)
      : base(logger, aIdentity, defaultConversationalConnector, defaultCompletionConnector, skillManager)
   {
      _replySkill = skillManager.Get<ReplyToPrompt>();
   }

   public override CognitiveContext CreateCognitiveContext() => new CognitiveContext(this);

   public override async IAsyncEnumerable<Thought> HandlePromptAsync(
      Prompt prompt,
      MissionContext? missionContext,
      [EnumeratorCancellation] CancellationToken cancellationToken)
   {

      if (_replySkill is null)
      {
         yield return FinalThought("It seems I'm unable to reply...");
         yield break;
      }

      SkillExecutionContext? skillExecutionContext = null;
      if (missionContext is not null)
      {
         skillExecutionContext = new SkillExecutionContext(_replySkill, Context, missionContext);
      }

      await foreach (var item in ExecuteSkill(_replySkill, prompt, skillExecutionContext, cancellationToken).ConfigureAwait(false))
      {
         yield return item;
      }
   }
}

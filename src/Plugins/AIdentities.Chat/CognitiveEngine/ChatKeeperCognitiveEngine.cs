using AIdentities.Shared.Features.CognitiveEngine.Engines.Reflexive;
using AIdentities.Shared.Plugins.Connectors.Completion;
using Polly;

namespace AIdentities.Chat.CognitiveEngine;

/// <summary>
/// This cognitive engine is used by chat keepers, AIdentities whose purpose is to control a conversation
/// performing any skill they can detect, delegating to other AIdentities in case no skill is detected.
/// Also they can be used to moderate a conversation between multiple AIdentities by giving them 
/// inputs to talk based on turns or other rules that you can implement by handling the 
/// <see cref="HandleNoCommandDetected(Prompt, IMissionContext?, CancellationToken)"/> method.
/// The model keep in memory the conversation pieces in its CognitiveContext in order to be able to iterate with the user about the same topic.
/// Skills can access to this memory to perform their tasks.
/// </summary>
public class ChatKeeperCognitiveEngine : ReflexiveCognitiveEngine<CognitiveContext>
{
   public ChatKeeperCognitiveEngine(ILogger<ChatKeeperCognitiveEngine> logger,
                                 AIdentity aIdentity,
                                 IConversationalConnector defaultConversationalConnector,
                                 ICompletionConnector defaultCompletionConnector,
                                 ISkillManager skillManager)
      : base(logger, aIdentity, defaultConversationalConnector, defaultCompletionConnector, skillManager) { }

   public override CognitiveContext CreateCognitiveContext() => new CognitiveContext(this);

   /// <summary>
   /// when no command is detected, pass the prompt to one of the current AIdentities in the conversation
   /// if no AIdentity is available then do nothing
   /// </summary>
   /// <inheritdoc/>
   protected override IAsyncEnumerable<Thought> HandleNoCommandDetected(
      Prompt prompt,
      IMissionContext? missionContext,
      CancellationToken cancellationToken)
      => KeepConversationGoing(prompt, missionContext, cancellationToken).Prepend(ActionThought("I haven't detected any command to execute, let them talk."));

   protected override IAsyncEnumerable<Thought> HandleUnknownCommandDetected(
      Prompt prompt,
      string detectedSkillName,
      IMissionContext? missionContext,
      CancellationToken cancellationToken)
      => KeepConversationGoing(prompt, missionContext, cancellationToken).Prepend(ActionThought($"I thought I found the skill {detectedSkillName} but I was wrong, it doesn't exists."));

   /// <summary>
   /// Keep the conversation going.
   /// It works if missionContext is a <see cref="CognitiveChatMissionContext"/> and there are
   /// <see cref="ParticipatingAIdentities"/> in the conversation.
   /// </summary>
   /// <param name="prompt">The prompt to pass to the AIdentity</param>
   /// <param name="missionContext">The mission context (should be a <see cref="CognitiveChatMissionContext"/>)</param>
   /// <param name="cancellationToken">The cancellation token</param>
   /// <returns>A stream of thoughts</returns>
   protected virtual async IAsyncEnumerable<Thought> KeepConversationGoing(
      Prompt prompt,
      IMissionContext? missionContext,
      [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      // we want to handle only CognitiveChatMissionContext
      if (missionContext is not CognitiveChatMissionContext chatMissionContext) yield break;

      ICognitiveEngine? replyingAidentity = null;
      var aIdentityPrompt = prompt as AIdentityPrompt;
      chatMissionContext.ParticipatingAIdentities.TryGetValue(aIdentityPrompt?.AIdentityId ?? Guid.Empty, out var promptSender);

      if (chatMissionContext.IsModeratedModeEnabled
         && chatMissionContext.NextTalker is not null
         && chatMissionContext.ParticipatingAIdentities.TryGetValue(chatMissionContext.NextTalker.Id, out var nextTalkerParticipant))
      {
         replyingAidentity = nextTalkerParticipant.CognitiveEngine;
      }

      // TODO: pick a random AIdentity or implement a better algorithm to know who should talk
      // at the moment we pick the first AIdentity that is not the prompt sender
      replyingAidentity ??= chatMissionContext.ParticipatingAIdentities.Values.FirstOrDefault(p => p != promptSender)?.CognitiveEngine;

      if (replyingAidentity is null)
      {
         yield return ActionThought("There is no one that can talk here");
         yield break;
      }

      var thoughts = replyingAidentity.HandlePromptAsync(prompt, missionContext, cancellationToken).ConfigureAwait(false);
      await foreach (var thought in thoughts)
      {
         yield return thought;
      }
   }
}

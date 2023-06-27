using System.Diagnostics.CodeAnalysis;
using Fluid;

namespace AIdentities.Chat.Skills.IntroduceYourself;

public partial class IntroduceYourself : Skill
{
   /// <summary>
   /// We expect the conversation participants to be in the context with this key.
   /// </summary>
   public const string PARTICIPATING_AIDENTITIES_KEY = nameof(CognitiveChatMissionContext.ParticipatingAIdentities);

   public IntroduceYourself(ILogger<IntroduceYourself> logger, IAIdentityProvider aIdentityProvider, FluidParser templateParser)
      : base(logger, aIdentityProvider, templateParser) { }

   protected override void CreateDefaultPromptTemplates() { }

   protected override bool ValidateInputs(
      Prompt prompt,
      SkillExecutionContext context,
      [MaybeNullWhen(true)] out InvalidArgumentsThought error)
   {
      // no inputs to validate
      error = null;
      return true;
   }

   protected override async IAsyncEnumerable<Thought> ExecuteAsync(
      SkillExecutionContext context,
      [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      SetPresentation(context, null);

      // if a skill doesn't depend explicitly on a connector, it should use the one defined in the cognitive engine
      var connector = context.CognitiveContext.CognitiveEngine.GetDefaultConnector<IConversationalConnector>()
         ?? throw new InvalidOperationException("No completion connector is enabled");

      var aidentity = context.AIdentity;

      // check in the context if there is a conversation
      if (!TryExtractFromContext<Dictionary<Guid, ParticipatingAIdentity>>(PARTICIPATING_AIDENTITIES_KEY, context, out var participants))
      {
         yield return context.ActionThought("I don't have a conversation to examine, using the prompt instead");
      }

      var participantNames = participants?.Select(p => p.Value.AIdentity.Name) ?? Array.Empty<string>();
      if (!participantNames.Contains("User"))
      {
         participantNames = participantNames.Append("User");
      }

      var streamedResult = connector.RequestChatCompletionAsStreamAsync(new ConversationalRequest(aidentity)
      {
         Messages = PromptTemplates.GetIntroductionPrompt(aidentity, participantNames).ToList(),
         Temperature = 0.7f, //TODO: make this configurable based on the AIdentity
         MaxGeneratedTokens = 100 //TODO: make this configurable based on the AIdentity
      }, cancellationToken).ConfigureAwait(false);

      var streamedFinalThought = context.StreamFinalThought("");
      await foreach (var thought in streamedResult)
      {
         streamedFinalThought.AppendContent(thought.GeneratedMessage ?? "");
         yield return streamedFinalThought;
      }

      SetPresentation(context, streamedFinalThought.Content);

      yield return streamedFinalThought.Completed();
   }
}

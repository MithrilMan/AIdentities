using System.Diagnostics.CodeAnalysis;

namespace AIdentities.Chat.Skills.IntroduceYourself;

public partial class IntroduceYourself : Skill
{
   readonly ILogger<IntroduceYourself> _logger;

   public IntroduceYourself(ILogger<IntroduceYourself> logger)
   {
      _logger = logger;
   }

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

      var streamedResult = connector.RequestChatCompletionAsStreamAsync(new DefaultConversationalRequest(aidentity)
      {
         Messages = PromptTemplates.GetIntroductionPrompt(aidentity),
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

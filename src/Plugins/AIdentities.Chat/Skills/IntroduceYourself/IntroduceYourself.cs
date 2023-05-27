using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace AIdentities.Chat.Skills.IntroduceYourself;

public partial class IntroduceYourself : Skill
{
   readonly ILogger<IntroduceYourself> _logger;
   readonly IDefaultConnectors _defaultConnectors;

   public IntroduceYourself(ILogger<IntroduceYourself> logger, IDefaultConnectors defaultConnectors)
   {
      _logger = logger;
      _defaultConnectors = defaultConnectors;
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

      var connector = _defaultConnectors.DefaultConversationalConnector
         ?? throw new InvalidOperationException("No completion connector is enabled");

      var aidentity = context.AIdentity;

      var streamedResult = connector.RequestChatCompletionAsStreamAsync(new DefaultConversationalRequest
      {
         Messages = PromptTemplates.GetIntroductionPrompt(aidentity),
         Temperature = 0.7m, //TODO: make this configurable based on the AIdentity
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

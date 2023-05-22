using System.Runtime.CompilerServices;

namespace AIdentities.Chat.Skills.IntroduceYourself;

public class IntroduceYourself : SkillDefinition
{
   public const string NAME = nameof(IntroduceYourself);
   const string ACTIVATION_CONTEXT = "The AIdenity has to introduce itself to the conversation";
   const string RETURN_DESCRIPTION = "The sentence that the AIdentity will say to introduce itself";

   const string EXAMPLES = "";

   static readonly JsonSerializerOptions _jsonOptionExample = new() { WriteIndented = true };

   readonly ILogger<IntroduceYourself> _logger;
   readonly IDefaultConnectors _defaultConnectors;

   public IntroduceYourself(ILogger<IntroduceYourself> logger, IDefaultConnectors defaultConnectors)
      : base(NAME, ACTIVATION_CONTEXT, RETURN_DESCRIPTION, EXAMPLES)
   {
      _logger = logger;
      _defaultConnectors = defaultConnectors;
   }

   public override async IAsyncEnumerable<Thought> ExecuteAsync(Prompt prompt,
                                                                CognitiveContext cognitiveContext,
                                                                MissionContext? missionContext,
                                                                [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      var connector = _defaultConnectors.DefaultConversationalConnector
         ?? throw new InvalidOperationException("No completion connector is enabled");

      var aidentity = cognitiveContext.AIdentity;
      if (aidentity is null)
      {
         yield return cognitiveContext.InvalidPrompt(this);
         yield break;
      }


      var streamedResult = connector.RequestChatCompletionAsStreamAsync(new DefaultConversationalRequest
      {
         Messages = PromptTemplates.GetIntroductionPrompt(aidentity),
         Temperature = 0.7m, //TODO: make this configurable based on the AIdentity
         MaxGeneratedTokens = 100 //TODO: make this configurable based on the AIdentity
      }, cancellationToken).ConfigureAwait(false);

      var streamedFinalThought = new StreamedFinalThought(Id, aidentity.Id, "", false);
      await foreach (var thought in streamedResult)
      {
         streamedFinalThought.AppendContent(thought.GeneratedMessage ?? "");
         yield return streamedFinalThought;
      }
      streamedFinalThought.IsStreamComplete = true;
      yield return streamedFinalThought;
   }
}

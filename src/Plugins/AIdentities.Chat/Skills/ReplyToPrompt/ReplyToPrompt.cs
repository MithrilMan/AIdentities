using System.Runtime.CompilerServices;
using AIdentities.Shared.Features.CognitiveEngine.Memory.Conversation;

namespace AIdentities.Chat.Skills.ReplyToPrompt;
public class ReplyToPrompt : SkillDefinition
{
   public const string NAME = nameof(ReplyToPrompt);
   const string ACTIVATION_CONTEXT = "The AIdenity has to introduce itself to the conversation";
   const string RETURN_DESCRIPTION = "The sentence that the AIdentity will say to introduce itself";

   const string EXAMPLES = "";

   static readonly JsonSerializerOptions _jsonOptionExample = new() { WriteIndented = true };

   private readonly List<SkillArgumentDefinition> _arguments = new()
   {
      Args.ConversationContextDefinition
   };

   readonly ILogger<ReplyToPrompt> _logger;
   readonly IDefaultConnectors _defaultConnectors;
   readonly IPluginStorage<PluginEntry> _pluginStorage;
   readonly IAIdentityProvider _aIdentityProvider;

   public ReplyToPrompt(ILogger<ReplyToPrompt> logger,
                             IDefaultConnectors defaultConnectors,
                             IPluginStorage<PluginEntry> pluginStorage,
                             IAIdentityProvider aIdentityProvider
                             )
      : base(NAME, ACTIVATION_CONTEXT, RETURN_DESCRIPTION, EXAMPLES)
   {
      _logger = logger;
      _defaultConnectors = defaultConnectors;
      _pluginStorage = pluginStorage;
      _aIdentityProvider = aIdentityProvider;

      Arguments = _arguments;
   }

   public override async IAsyncEnumerable<Thought> ExecuteAsync(Prompt prompt,
                                                                CognitiveContext cognitiveContext,
                                                                MissionContext? missionContext,
                                                                [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      var connector = _defaultConnectors.DefaultConversationalConnector
         ?? throw new InvalidOperationException("No completion connector is enabled");

      var aidentity = cognitiveContext.AIdentity;

      // check if in the cognitive context there is a conversation history
      if (!TryExtractFromContext<IConversationHistory>(cognitiveContext, missionContext, out IConversationHistory? conversationHistory))
      {
         yield return cognitiveContext.ActionThought(this, "I don't have a chat history to examine, using the prompt instead");
      }

      (Guid authorId, bool isAiGenerated) = prompt switch
      {
         UserPrompt userPrompt => (userPrompt.UserId, false),
         AIdentityPrompt aIdentityPrompt => (aIdentityPrompt.AIdentityId, true),
         _ => (Guid.Empty, false)
      };

      var history = conversationHistory is not null ?
         conversationHistory.GetConversationHistory(aidentity)
         : new List<ConversationMessage>()
         {
            new ConversationMessage{
               AuthorId = authorId,
               IsAIGenerated = !isAiGenerated,
               Text=prompt.Text
            }
         };

      var streamedResult = connector.RequestChatCompletionAsStreamAsync(new DefaultConversationalRequest
      {
         Messages = PromptTemplates.BuildPromptMessages(aidentity, history),
         MaxGeneratedTokens = 200
      }, cancellationToken).ConfigureAwait(false);

      var streamedFinalThought = cognitiveContext.StreamFinalThought(this, "");
      await foreach (var thought in streamedResult)
      {
         streamedFinalThought.AppendContent(thought.GeneratedMessage ?? "");
         yield return streamedFinalThought;
      }

      yield return streamedFinalThought.Completed();
   }
}

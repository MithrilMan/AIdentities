using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using AIdentities.Shared.Features.CognitiveEngine.Memory.Conversation;

namespace AIdentities.Chat.Skills.ReplyToPrompt;
public partial class ReplyToPrompt : Skill
{
   static readonly JsonSerializerOptions _jsonOptionExample = new() { WriteIndented = true };

   readonly ILogger<ReplyToPrompt> _logger;
   readonly IDefaultConnectors _defaultConnectors;
   readonly IPluginStorage<PluginEntry> _pluginStorage;
   readonly IAIdentityProvider _aIdentityProvider;

   public ReplyToPrompt(ILogger<ReplyToPrompt> logger,
                             IDefaultConnectors defaultConnectors,
                             IPluginStorage<PluginEntry> pluginStorage,
                             IAIdentityProvider aIdentityProvider
                             )
   {
      _logger = logger;
      _defaultConnectors = defaultConnectors;
      _pluginStorage = pluginStorage;
      _aIdentityProvider = aIdentityProvider;

   }
  
   protected override async IAsyncEnumerable<Thought> ExecuteAsync(
      SkillExecutionContext context,
      [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      var connector = _defaultConnectors.DefaultConversationalConnector
         ?? throw new InvalidOperationException("No completion connector is enabled");

      var aidentity = context.AIdentity;

      // check if in the cognitive context there is a conversation history
      if (!TryExtractFromContext<IConversationHistory>(context, out IConversationHistory? conversationHistory))
      {
         yield return context.ActionThought("I don't have a chat history to examine, using the prompt instead");
      }

      var prompt = context.PromptChain.Peek();
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

      var streamedFinalThought = context.StreamFinalThought("");
      await foreach (var thought in streamedResult)
      {
         streamedFinalThought.AppendContent(thought.GeneratedMessage ?? "");
         yield return streamedFinalThought;
      }

      yield return streamedFinalThought.Completed();
   }
}

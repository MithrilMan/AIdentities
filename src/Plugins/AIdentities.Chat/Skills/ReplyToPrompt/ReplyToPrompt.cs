namespace AIdentities.Chat.Skills.ReplyToPrompt;

public partial class ReplyToPrompt : Skill
{
   /// <summary>
   /// We expect the conversation history to be in the context with this key.
   /// </summary>
   public const string CONVERSATION_HISTORY_KEY = nameof(CognitiveChatMissionContext.ConversationHistory);
   /// <summary>
   /// If this contextual variable is set, the skill will reply to the message instead of the last message conversation.
   /// </summary>
   public const string MESSAGE_TO_REPLY_TO_KEY = nameof(CognitiveChatMissionContext.MessageToReplyTo);

   static readonly JsonSerializerOptions _jsonOptionExample = new() { WriteIndented = true };

   readonly ILogger<ReplyToPrompt> _logger;
   readonly IAIdentityProvider _aIdentityProvider;

   public ReplyToPrompt(ILogger<ReplyToPrompt> logger, IAIdentityProvider aIdentityProvider)
   {
      _logger = logger;
      _aIdentityProvider = aIdentityProvider;

   }

   protected override async IAsyncEnumerable<Thought> ExecuteAsync(
      SkillExecutionContext context,
      [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      // if a skill doesn't depend explicitly on a connector, it should use the one defined in the cognitive engine
      var connector = context.CognitiveContext.CognitiveEngine.GetDefaultConnector<IConversationalConnector>()
         ?? throw new InvalidOperationException("No completion connector is enabled");

      var aidentity = context.AIdentity;

      // check in the context if there is a conversation history
      if (!TryExtractFromContext<IConversationHistory>(CONVERSATION_HISTORY_KEY, context, out var conversationHistory))
      {
         yield return context.ActionThought("I don't have a chat history to examine, using the prompt instead");
      }

      // check in the con
      if (TryExtractFromContext<ConversationMessage>(MESSAGE_TO_REPLY_TO_KEY, context, out var messageToReplyToKey))
      {
         yield return context.ActionThought($"The AIdentity {aidentity.Name} is replying to a specific message");
      }

      IEnumerable<ConversationMessage> history = Array.Empty<ConversationMessage>();
      if (conversationHistory is not null)
      {
         history = conversationHistory.GetConversationHistory(aidentity, messageToReplyToKey);
      }
      else
      {
         var prompt = context.PromptChain.Peek();
         var conversationMessage = prompt switch
         {
            UserPrompt userPrompt => new ConversationMessage(prompt.Text, userPrompt.UserId, "User"),
            AIdentityPrompt aIdentityPrompt => new ConversationMessage(prompt.Text, _aIdentityProvider.Get(aIdentityPrompt.AIdentityId) ?? aidentity),
            _ => null
         };
         if (conversationMessage is not null)
         {
            history = new ConversationMessage[] { conversationMessage };
         }
      }

      var streamedResult = connector.RequestChatCompletionAsStreamAsync(new DefaultConversationalRequest
      {
         Messages = PromptTemplates.BuildPromptMessages(aidentity, history),
         //MaxGeneratedTokens = 200
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

using Fluid;

namespace AIdentities.Chat.Skills.ReplyToPrompt;

public partial class ReplyToPrompt : Skill
{
   /// <summary>
   /// We expect the conversation history to be in the context with this key.
   /// </summary>
   public const string CONVERSATION_HISTORY_KEY = nameof(CognitiveChatMissionContext.ConversationHistory);

   /// <summary>
   /// We expect the conversation participants to be in the context with this key.
   /// </summary>
   public const string PARTICIPATING_AIDENTITIES_KEY = nameof(CognitiveChatMissionContext.ParticipatingAIdentities);

   /// <summary>
   /// If this contextual variable is set, the skill will reply to the message instead of the last message conversation.
   /// </summary>
   public const string MESSAGE_TO_REPLY_TO_KEY = nameof(CognitiveChatMissionContext.MessageToReplyTo);

   public ReplyToPrompt(ILogger<ReplyToPrompt> logger, IAIdentityProvider aIdentityProvider, FluidParser templateParser)
      : base(logger, aIdentityProvider, templateParser) { }

   protected override void CreateDefaultPromptTemplates() { }

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

      // check in the context if there is a conversation
      if (!TryExtractFromContext<Dictionary<Guid, ParticipatingAIdentity>>(PARTICIPATING_AIDENTITIES_KEY, context, out var participants))
      {
         yield return context.ActionThought("I don't have a conversation to examine, using the prompt instead");
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
            AIdentityPrompt aIdentityPrompt => new ConversationMessage(prompt.Text, AIdentityProvider.Get(aIdentityPrompt.AIdentityId) ?? aidentity),
            _ => null
         };
         if (conversationMessage is not null)
         {
            history = new ConversationMessage[] { conversationMessage };
         }
      }

      var participantNames = participants?.Select(p => p.Value.AIdentity.Name) ?? Array.Empty<string>();
      if (!participantNames.Contains("User"))
      {
         participantNames = participantNames.Append("User");
      }

      var streamedResult = connector.RequestChatCompletionAsStreamAsync(new ConversationalRequest(aidentity)
      {
         Messages = PromptTemplates.BuildPromptMessages(
            aidentity,
            history,
            participantNames
            ).ToList(),
         StopSequences = participantNames.Select(p => $"\n{p}:").ToList()
         //MaxGeneratedTokens = 200
      }, cancellationToken).ConfigureAwait(false);

      var streamedFinalThought = context.StreamFinalThought("");
      await foreach (var thought in streamedResult)
      {
         streamedFinalThought.AppendContent(thought.GeneratedMessage ?? "");
         yield return streamedFinalThought;
      }

      yield return streamedFinalThought.Completed(
         CleanMessage(streamedFinalThought.Content, participantNames)
         );
   }

   public static string CleanMessage(string message, IEnumerable<string> participantNames)
   {
      var cleanedMessage = message;
      foreach (var participantName in participantNames)
      {
         cleanedMessage = cleanedMessage.Replace($"\n{participantName}:", "");
      }
      return cleanedMessage.Trim();
   }
}

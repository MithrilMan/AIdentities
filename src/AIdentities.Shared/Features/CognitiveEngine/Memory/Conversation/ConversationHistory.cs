namespace AIdentities.Shared.Features.CognitiveEngine.Memory.Conversation;

public class ConversationHistory : IConversationHistory
{
   readonly ILogger<ConversationHistory> _logger;

   public Conversation? CurrentConversation { get; private set; }


   public ConversationHistory(ILogger<ConversationHistory> logger)
   {
      _logger = logger;
   }

   public IEnumerable<ConversationMessage> GetConversationHistory(AIdentity pointOfView)
   {
      if (CurrentConversation == null) throw new InvalidOperationException("No conversation is loaded.");

      // we'd need to know the time when the AIdentity joined the conversation
      // since we don't know, we can't actually skip messages before the AIdentity joined
      //var messages = CurrentConversation
      //   .Messages
      //   .SkipWhile(m => !m.IsAIGenerated || m.AuthorId != pointOfView.Id);

      var messages = CurrentConversation.Messages;

      return messages;
   }

   public void SetConversation(Conversation conversation)
   {
      CurrentConversation = conversation;
   }
}

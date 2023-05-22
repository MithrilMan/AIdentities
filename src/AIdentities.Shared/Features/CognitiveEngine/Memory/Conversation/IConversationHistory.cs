using AIdentities.Shared.Plugins.Connectors.Conversational;

namespace AIdentities.Shared.Features.CognitiveEngine.Memory.Conversation;

/// <summary>
/// Stores a conversation history.
/// The history can be manipulated depending on the implementation,
/// could for example be summarized to save tokens, could be retrieved externally, etc.
/// </summary>
public interface IConversationHistory
{
   /// <summary>
   /// Returns the conversation history from the point of view of the given aIdentity.
   /// </summary>
   /// <param name="pointOfView">The aIdentity to get the conversation history from.</param>
   /// <returns>The conversation history from the point of view of the given aIdentity.</returns>
   IEnumerable<ConversationMessage> GetConversationHistory(AIdentity pointOfView);
}

public class ConversationHistory : IConversationHistory
{
   readonly ILogger<ConversationHistory> _logger;

   readonly List<ConversationMessage> _conversationHistory = new();

   public ConversationHistory(ILogger<ConversationHistory> logger)
   {
      _logger = logger;
   }

   public IEnumerable<ConversationMessage> GetConversationHistory(AIdentity pointOfView)
   {
      return _conversationHistory
         .SkipWhile(m => !m.IsAIGenerated || m.AuthorId != pointOfView.Id);
   }
}

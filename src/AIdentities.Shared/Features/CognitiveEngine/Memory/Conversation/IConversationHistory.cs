using AIdentities.Shared.Plugins.Connectors.Conversational;

namespace AIdentities.Shared.Features.CognitiveEngine.Memory.Conversation;

/// <summary>
/// Manages a conversation history.
/// The history can be manipulated depending on the implementation,
/// could for example be summarized to save tokens, could be retrieved externally, etc.
/// </summary>
public interface IConversationHistory
{
   /// <summary>
   /// The current conversation.
   /// </summary>
   public Conversation? CurrentConversation { get; }

   /// <summary>
   /// Sets the conversation to manage.
   /// </summary>
   /// <param name="conversation">The conversation to manage.</param>
   void SetConversation(Conversation? conversation);

   /// <summary>
   /// Returns the conversation history from the point of view of the given aIdentity.
   /// </summary>
   /// <param name="pointOfView">The aIdentity to get the conversation history from.</param>
   /// <param name="mostRecentMessageToTake">The history will stop at the specified message. Messages
   /// after this one will not be returned.</param>
   /// <returns>The conversation history from the point of view of the given aIdentity.</returns>
   IEnumerable<ConversationMessage> GetConversationHistory(AIdentity pointOfView, ConversationMessage? mostRecentMessageToTake);
}

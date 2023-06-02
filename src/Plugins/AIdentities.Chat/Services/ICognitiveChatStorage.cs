namespace AIdentities.Chat.Services;

public interface ICognitiveChatStorage
{
   /// <summary>
   /// Gets all stored conversations.
   /// </summary>
   /// <returns>A list of stored conversations.</returns>
   ValueTask<IEnumerable<Conversation>> GetConversationsAsync();

   /// <summary>
   /// Gets all stored conversations for a given AIdentity.
   /// </summary>
   /// <returns>A list of stored conversations held by the given AIdentity.</returns>
   ValueTask<IEnumerable<Conversation>> GetConversationsByAIdentityAsync(AIdentity aIdentity);

   /// <summary>
   /// Loads a complete conversation by its id.
   /// </summary>
   /// <param name="conversationId">The conversation id.</param>
   /// <returns>The loaded conversation.</returns>
   ValueTask<Conversation> LoadConversationAsync(Guid conversationId);

   /// <summary>
   /// Starts a new conversation.
   /// </summary>
   /// <param name="conversation">The conversation to start.</param>
   ValueTask StartConversationAsync(Conversation conversation);

   /// <summary>
   /// Updates a conversation with a new message.
   /// </summary>
   /// <param name="conversation">The conversation.</param>
   /// <param name="message">The message to add.
   /// If null, the conversation only will be updated.
   /// </param>
   /// <returns>True if the conversation was updated, false otherwise.</returns>
   ValueTask<bool> UpdateConversationAsync(Conversation conversation, ConversationMessage? message);

   /// <summary>
   /// Deletes a conversation.
   /// </summary>
   /// <param name="conversationId">The conversation id.</param>
   /// <returns>True if the conversation was deleted, false otherwise.</returns>
   ValueTask<bool> DeleteConversationAsync(Guid conversationId);

   /// <summary>
   /// Deletes a message from a conversation and update the.
   /// </summary>
   /// <param name="conversation">The conversation.</param>
   /// <param name="message">The message to delete.</param>
   /// <returns>True if the message was deleted, false otherwise.</returns>
   ValueTask<bool> DeleteMessageAsync(Conversation conversation, ConversationMessage message);

   /// <summary>
   /// Clears all conversation messages.
   /// All participants will still be available.
   /// </summary>
   ValueTask ClearConversation(Conversation conversation);
}

﻿namespace AIdentities.Chat.Services;

public interface IChatStorage
{
   /// <summary>
   /// Gets all stored conversations metadata.
   /// </summary>
   /// <returns>A list of stored conversations metadata.</returns>
   ValueTask<IEnumerable<ConversationMetadata>> GetConversationsAsync();

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
   /// <param name="conversationMetadata">The conversation metadata.</param>
   /// <param name="message">The message to add.
   /// If null, the conversation metadata only will be updated.
   /// </param>
   /// <returns>True if the conversation was updated, false otherwise.</returns>
   ValueTask<bool> UpdateConversationAsync(ConversationMetadata conversationMetadata, ChatMessage? message);

   /// <summary>
   /// Deletes a conversation.
   /// </summary>
   /// <param name="conversationId">The conversation id.</param>
   /// <returns>True if the conversation was deleted, false otherwise.</returns>
   ValueTask<bool> DeleteConversationAsync(Guid conversationId);
}
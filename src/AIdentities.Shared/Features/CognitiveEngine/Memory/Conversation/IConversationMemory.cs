namespace AIdentities.Shared.Features.CognitiveEngine.Memory.Conversation;

/// <summary>
/// Allows to manage the memory of a single conversation.
/// It could be used to store the conversation in a database or in memory,
/// the implementation is up to the developer.
/// </summary>
interface IInMemoryConversation
{
   ValueTask<Conversation> CreateNewAsync();

   ValueTask LoadConversationAsync(Guid conversationId);

   ValueTask SaveAsync();

   ValueTask DeleteAsync();

   ValueTask AddMessageAsync(ConversationMessage message);

   ValueTask RemoveMessageAsync(Guid messageId);

   ValueTask AddAIdentityAsync(AIdentity aIdentity);

   ValueTask RemoveAIdentityAsync(Guid aIdentityId);

   ValueTask RemoveAIdentityAsync(AIdentity aIdentity);

   ValueTask<Conversation> GetConversationAsync(Guid conversationId);
}

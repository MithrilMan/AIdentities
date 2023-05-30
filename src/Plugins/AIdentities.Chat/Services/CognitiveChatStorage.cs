using AIdentities.Chat.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AIdentities.Chat.Services;

public class CognitiveChatStorage : ICognitiveChatStorage
{
   readonly ILogger<CognitiveChatStorage> _logger;
   readonly ConversationDbContext _dbContext;

   public CognitiveChatStorage(ILogger<CognitiveChatStorage> logger, ConversationDbContext dbContext)
   {
      _logger = logger;
      _dbContext = dbContext;
   }

   public async ValueTask<bool> DeleteConversationAsync(Guid conversationId)
   {
      using var trx = _dbContext.Database.BeginTransaction();

      var convesation = _dbContext.Conversations.Find(conversationId);
      if (convesation == null) return false;

      _dbContext.Conversations.Remove(convesation);
      await _dbContext.SaveChangesAsync().ConfigureAwait(false);

      await trx.CommitAsync().ConfigureAwait(false);

      return true;
   }

   public async ValueTask<IEnumerable<Conversation>> GetConversationsAsync()
   {
      return await _dbContext.Conversations.ToListAsync().ConfigureAwait(false);
   }

   public async ValueTask<IEnumerable<Conversation>> GetConversationsByAIdentityAsync(AIdentity aIdentity)
   {
      return await _dbContext.Conversations
         .Where(m => m.AIdentityIds.Contains(aIdentity.Id))
         .ToListAsync()
         .ConfigureAwait(false);
   }

   public async ValueTask<Conversation> LoadConversationAsync(Guid conversationId)
   {
      try
      {
         return (await _dbContext
            .Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == conversationId)
            .ConfigureAwait(false)
            )!;
      }
      catch (Exception ex)
      {
         _logger.LogError(ex, "Conversation with id {conversationId} is corrupted.", conversationId);
         throw new FormatException($"Conversation with id {conversationId} is corrupted.", ex);
      }
   }

   public async ValueTask<bool> UpdateConversationAsync(Conversation conversation, ConversationMessage? message)
   {
      using var trx = _dbContext.Database.BeginTransaction();

      if (message != null)
      {
         conversation.AddMessage(message);
      }

      _dbContext.Conversations.Update(conversation);
      await _dbContext.SaveChangesAsync().ConfigureAwait(false);
      await trx.CommitAsync().ConfigureAwait(false);

      return true;
   }

   public async ValueTask<bool> DeleteMessageAsync(Conversation Conversation, ConversationMessage message)
   {
      using var trx = _dbContext.Database.BeginTransaction();

      if (message != null)
      {
         //bool removed = _dbContext.ConversationMessages.Remove(message) != null;
         //if (removed)
         //{
         //   Conversation.MessageCount--;
         //}
         Conversation.RemoveMessage(message.Id);
         _dbContext.Conversations.Update(Conversation);
      }
      await _dbContext.SaveChangesAsync().ConfigureAwait(false);
      await trx.CommitAsync().ConfigureAwait(false);

      return true;
   }

   public async ValueTask StartConversationAsync(Conversation conversation)
   {
      using var trx = _dbContext.Database.BeginTransaction();

      _dbContext.Conversations.Add(conversation);
      await _dbContext.SaveChangesAsync().ConfigureAwait(false);
      await trx.CommitAsync().ConfigureAwait(false);
   }
}

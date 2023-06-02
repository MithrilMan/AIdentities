using Microsoft.EntityFrameworkCore;

namespace AIdentities.Chat.Persistence;

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

      var convesation = await _dbContext
         .Conversations
         .FirstOrDefaultAsync(c => c.Id == conversationId)
         .ConfigureAwait(false);

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
         return (await _dbContext.Conversations.FindAsync(conversationId).ConfigureAwait(false))!;
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

      _dbContext.Conversations.Attach(conversation);
      if (message != null)
      {
         conversation.AddMessage(message);
         _dbContext.Entry(message).State = EntityState.Added;
      }

      await _dbContext.SaveChangesAsync().ConfigureAwait(false);
      await trx.CommitAsync().ConfigureAwait(false);

      return true;
   }

   public async ValueTask ClearConversation(Conversation conversation)
   {
      using var trx = _dbContext.Database.BeginTransaction();

      _dbContext.Conversations.Attach(conversation);
      conversation.Clear();

      await _dbContext.SaveChangesAsync().ConfigureAwait(false);
      await trx.CommitAsync().ConfigureAwait(false);
   }

   public async ValueTask<bool> DeleteMessageAsync(Conversation conversation, ConversationMessage message)
   {
      using var trx = _dbContext.Database.BeginTransaction();

      _dbContext.Conversations.Attach(conversation);
      if (message != null)
      {
         if (conversation.RemoveMessage(message.Id))
            _dbContext.Entry(message).State = EntityState.Deleted;
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

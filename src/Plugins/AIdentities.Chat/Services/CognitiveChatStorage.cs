using AIdentities.Chat.Persistence;
using AIdentities.Shared.Features.AIdentities.Models;
using AIdentities.Shared.Features.CognitiveEngine.Memory.Conversation;
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

      _dbContext.Conversations.Remove(new Conversation { Id = conversationId });
      await _dbContext.SaveChangesAsync().ConfigureAwait(false);

      await trx.CommitAsync().ConfigureAwait(false);

      return true;
   }

   public async ValueTask<IEnumerable<ConversationMetadata>> GetConversationsAsync()
   {
      return await _dbContext.ConversationMetadata.ToListAsync().ConfigureAwait(false);
   }

   public async ValueTask<IEnumerable<ConversationMetadata>> GetConversationsByAIdentityAsync(AIdentity aIdentity)
   {
      return await _dbContext.ConversationMetadata
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
            .Include(c => c.Metadata)
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

   public async ValueTask<bool> UpdateConversationAsync(ConversationMetadata conversationMetadata, ConversationMessage? message)
   {
      using var trx = _dbContext.Database.BeginTransaction();

      if (message != null)
      {
         conversationMetadata.MessageCount++;
         _dbContext.ConversationMessages.Add(message);
      }

      _dbContext.ConversationMetadata.Update(conversationMetadata);
      await _dbContext.SaveChangesAsync().ConfigureAwait(false);
      await trx.CommitAsync().ConfigureAwait(false);

      return true;
   }

   public async ValueTask<bool> DeleteMessageAsync(ConversationMetadata conversationMetadata, ConversationMessage message)
   {
      using var trx = _dbContext.Database.BeginTransaction();

      if (message != null)
      {
         bool removed = _dbContext.ConversationMessages.Remove(message) != null;
         if (removed)
         {
            conversationMetadata.MessageCount--;
         }
         _dbContext.ConversationMetadata.Update(conversationMetadata);
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

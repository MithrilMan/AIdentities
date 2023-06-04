using AIdentities.Chat.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AIdentities.Chat.Services;

public class StartupService : IPluginStartup
{
   readonly ConversationDbContext _dbContext;

   public StartupService(ConversationDbContext dbContext)
   {
      _dbContext = dbContext;
   }

   public async ValueTask StartupAsync()
   {
      await _dbContext.Database.MigrateAsync().ConfigureAwait(false);
   }
}

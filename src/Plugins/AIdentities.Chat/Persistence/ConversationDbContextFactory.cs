using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging.Abstractions;

namespace AIdentities.Chat.Persistence;

/// <summary>
/// This class is used only to generate migrations, we don't need real services in the
/// ConversationDbContext constructor.
/// </summary>
public class ConversationDbContextFactory : IDesignTimeDbContextFactory<ConversationDbContext>
{
   public ConversationDbContext CreateDbContext(string[] args)
   {
      var optionsBuilder = new DbContextOptionsBuilder<ConversationDbContext>();
      optionsBuilder.UseSqlite("Data Source=blog.db");

      var loggerFactory = new NullLoggerFactory();

      return new ConversationDbContext(loggerFactory.CreateLogger<ConversationDbContext>(), null!, loggerFactory);
   }
}

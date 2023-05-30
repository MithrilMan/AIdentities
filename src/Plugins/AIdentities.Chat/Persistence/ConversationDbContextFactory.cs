using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AIdentities.Chat.Persistence;
public class ConversationDbContextFactory : IDesignTimeDbContextFactory<ConversationDbContext>
{
   public ConversationDbContext CreateDbContext(string[] args)
   {
      var optionsBuilder = new DbContextOptionsBuilder<ConversationDbContext>();
      optionsBuilder.UseSqlite("Data Source=blog.db");

      return new ConversationDbContext(null, null);
   }
}

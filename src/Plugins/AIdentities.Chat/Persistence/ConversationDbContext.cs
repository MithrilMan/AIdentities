using AIdentities.Chat.Persistence.EntityConfigurations;
using Microsoft.EntityFrameworkCore;

namespace AIdentities.Chat.Persistence;
public class ConversationDbContext : DbContext
{
   readonly ILogger<ConversationDbContext> _logger;
   readonly IPluginStorage<PluginEntry> _pluginStorage;
   readonly ILoggerFactory _loggerFactory;

   public ConversationDbContext(ILogger<ConversationDbContext> logger,
                                IPluginStorage<PluginEntry> pluginStorage,
                                ILoggerFactory loggerFactory)
   {
      _logger = logger;
      _pluginStorage = pluginStorage;
      _loggerFactory = loggerFactory;
   }

   public DbSet<Conversation> Conversations { get; set; }
   //public DbSet<ConversationMessage> ConversationMessages { get; set; }

   protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
   {
      base.OnConfiguring(optionsBuilder);

      var logger = _loggerFactory.CreateLogger("Microsoft.EntityFrameworkCore");

      optionsBuilder
         .UseSqlite($"Data Source={_pluginStorage?.GetPluginStoragePath()}/conversations.db")
         //.UseLoggerFactory(_loggerFactory)
         .EnableSensitiveDataLogging();
   }

   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
      base.OnModelCreating(modelBuilder);

      modelBuilder.ApplyConfiguration(new ConversationTypeConfiguration());
      modelBuilder.ApplyConfiguration(new ConversationMessageTypeConfiguration());
   }

   protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
   {
      configurationBuilder.Properties<Guid>().HaveConversion<string>();
   }
}

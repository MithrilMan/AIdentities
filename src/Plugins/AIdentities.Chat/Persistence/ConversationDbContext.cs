using AIdentities.Chat.Persistence.EntityConfigurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AIdentities.Chat.Persistence;
public class ConversationDbContext : DbContext
{
   readonly ILogger<ConversationDbContext> _logger;
   readonly IPluginStorage<PluginEntry> _pluginStorage;

   public ConversationDbContext(ILogger<ConversationDbContext> logger, IPluginStorage<PluginEntry> pluginStorage)
   {
      _logger = logger;
      _pluginStorage = pluginStorage;
   }

   public DbSet<Conversation> Conversations { get; set; }
   //public DbSet<ConversationMessage> ConversationMessages { get; set; }

   protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
   {
      optionsBuilder.UseSqlite($"Data Source={_pluginStorage?.GetPluginStoragePath()}/conversations.db");
   }

   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
      base.OnModelCreating(modelBuilder);
      //modelBuilder.Entity<Conversation>()
      //   .HasOne<ConversationMetadata>(c => c.Metadata)
      //   .WithOne<Conversation>(c => c.ConversationId)
      //    .Property(e => e.Metadata)
      //    .one
      //    .HasConversion(
      //        v => JsonSerializer.Serialize(v, null),
      //        v => JsonSerializer.Deserialize<ICollection<MyGuidEntity>>(v, null),

      //    );

      modelBuilder.ApplyConfiguration(new ConversationTypeConfiguration());
      modelBuilder.ApplyConfiguration(new ConversationMessageTypeConfiguration());
   }

   protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
   {
      configurationBuilder.Properties<Guid>().HaveConversion<string>();
   }
}

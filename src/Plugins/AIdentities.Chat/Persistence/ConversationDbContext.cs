using Microsoft.EntityFrameworkCore;

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
   public DbSet<ConversationMessage> ConversationMessages { get; set; }
   public DbSet<ConversationMetadata> ConversationMetadata { get; set; }

   protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
   {
      optionsBuilder.UseSqlite($"Data Source={_pluginStorage?.GetPluginStoragePath()}/conversations.db");
   }

   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
      //modelBuilder.Entity<Conversation>()
      //   .HasOne<ConversationMetadata>(c => c.Metadata)
      //   .WithOne<Conversation>(c => c.ConversationId)
      //    .Property(e => e.Metadata)
      //    .one
      //    .HasConversion(
      //        v => JsonSerializer.Serialize(v, null),
      //        v => JsonSerializer.Deserialize<ICollection<MyGuidEntity>>(v, null),

      //    );



      modelBuilder.Entity<ConversationMetadata>().HasKey(x => x.ConversationId);

      modelBuilder.Entity<ConversationMetadata>()
         .Property(e => e.AIdentityIds)
         .HasConversion(
               v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
               v => JsonSerializer.Deserialize<ICollection<Guid>>(v, (JsonSerializerOptions?)null)!
         );

      modelBuilder.Entity<ConversationMetadata>()
         .HasOne<Conversation>()
         .WithOne(c => c.Metadata)
         .HasForeignKey<ConversationMetadata>(cm => cm.ConversationId);

      modelBuilder.Entity<ConversationMetadata>()
         .Property(e => e.Humans)
         .HasConversion(
               v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
               v => JsonSerializer.Deserialize<ICollection<Guid>>(v, (JsonSerializerOptions?)null)!
         );


      modelBuilder.Entity<ConversationMetadata>().Ignore(c => c.Features);

      modelBuilder.Entity<ConversationMessage>().Ignore(c => c.Features);
   }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIdentities.Chat.Persistence.EntityConfigurations;

public class ConversationTypeConfiguration : IEntityTypeConfiguration<Conversation>
{
   private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
   {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      WriteIndented = true,
      AllowTrailingCommas = true,
      PropertyNameCaseInsensitive = true
   };

   public void Configure(EntityTypeBuilder<Conversation> builder)
   {
      builder.HasKey(b => b.Id);

      builder.Property(e => e.AIdentityIds)
         .HasColumnName("AIdentityIds")
         .HasSequenceJsonConversion<Guid, IReadOnlyCollection<Guid>, HashSet<Guid>>(_jsonOptions)
         .Metadata.SetPropertyAccessMode(PropertyAccessMode.Field);

      builder.Property(e => e.Humans)
         .HasColumnName("Humans")
         .HasSequenceJsonConversion<Guid, IReadOnlyCollection<Guid>, HashSet<Guid>>(_jsonOptions)
         .Metadata.SetPropertyAccessMode(PropertyAccessMode.Field);

      builder.Ignore(c => c.Features);
   }
}

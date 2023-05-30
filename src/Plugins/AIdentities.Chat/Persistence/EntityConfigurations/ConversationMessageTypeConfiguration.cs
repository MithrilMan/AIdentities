using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIdentities.Chat.Persistence.EntityConfigurations;

public class ConversationMessageTypeConfiguration : IEntityTypeConfiguration<ConversationMessage>
{
   private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
   {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      WriteIndented = true,
      AllowTrailingCommas = true,
      PropertyNameCaseInsensitive = true
   };

   public void Configure(EntityTypeBuilder<ConversationMessage> builder)
   {
      builder.Ignore(c => c.Features);
   }
}

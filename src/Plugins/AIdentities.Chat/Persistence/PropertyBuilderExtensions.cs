using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AIdentities.Chat.Persistence;

public static class PropertyBuilderExtensions
{
   private static readonly JsonSerializerOptions _defaultSerializerOptions = new JsonSerializerOptions
   {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      WriteIndented = true,
      AllowTrailingCommas = true,
      PropertyNameCaseInsensitive = true
   };

   public static PropertyBuilder<T?> HasJsonConversion<T>(this PropertyBuilder<T?> propertyBuilder, JsonSerializerOptions options) where T : class
   {
      options ??= _defaultSerializerOptions;

      ValueConverter<T?, string> converter = new ValueConverter<T?, string>
      (
         v => JsonSerializer.Serialize(v, options),
         v => JsonSerializer.Deserialize<T>(v, options)
      );

      ValueComparer<T?> comparer = new ValueComparer<T?>
      (
         (l, r) => JsonSerializer.Serialize(l, options) == JsonSerializer.Serialize(r, options),
         v => v == null ? 0 : JsonSerializer.Serialize(v, options).GetHashCode(),
         v => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(v, options), options)
      );

      propertyBuilder.HasConversion(converter, comparer);

      return propertyBuilder;
   }


   public static PropertyBuilder<TSequence> HasSequenceJsonConversion<TItem, TSequence, TConcreteCollection>(this PropertyBuilder<TSequence> propertyBuilder, JsonSerializerOptions options)
      where TSequence : IEnumerable<TItem>
      where TConcreteCollection : class, TSequence
   {
      options ??= _defaultSerializerOptions;

      ValueConverter<TSequence, string> converter = new ValueConverter<TSequence, string>
      (
         v => JsonSerializer.Serialize(v, options),
         v => JsonSerializer.Deserialize<TConcreteCollection>(v, options)!
      );

      ValueComparer<TSequence?> comparer = new ValueComparer<TSequence?>
      (
         equalsExpression: (l, r) => JsonSerializer.Serialize(l, options) == JsonSerializer.Serialize(r, options),// l.SequenceEqual(r),
         hashCodeExpression: v => v == null ? 0 : JsonSerializer.Serialize(v, options).GetHashCode(), //v.Aggregate(0, (acc, item) => HashCode.Combine(acc, item.GetHashCode())),
         snapshotExpression: v => JsonSerializer.Deserialize<TConcreteCollection>(JsonSerializer.Serialize(v, options), options)
      );

      propertyBuilder.HasConversion(converter, comparer);

      return propertyBuilder;
   }
}

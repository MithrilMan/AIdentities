using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Features;

namespace AIdentities.Shared.Serialization;

public class FeatureCollectionJsonConverter : JsonConverter<FeatureCollection>
{
   public override FeatureCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
   {
      if (reader.TokenType != JsonTokenType.StartObject)
      {
         throw new JsonException("Expected a JSON object.");
      }

      var featureCollection = new FeatureCollection();

      while (reader.Read())
      {
         if (reader.TokenType == JsonTokenType.EndObject)
         {
            return featureCollection;
         }

         if (reader.TokenType != JsonTokenType.PropertyName)
         {
            throw new JsonException("Expected a JSON property name.");
         }

         string featureTypeName = reader.GetString()!;
         Type featureType = Type.GetType(featureTypeName) ?? throw new JsonException($"Unable to resolve the feature type '{featureTypeName}'.");
         reader.Read();
         object featureInstance = JsonSerializer.Deserialize(ref reader, featureType, options)!;

         if (featureInstance != null)
         {
            featureCollection[featureType] = featureInstance;
         }
      }

      throw new JsonException("Expected a JSON object.");
   }

   public override void Write(Utf8JsonWriter writer, FeatureCollection value, JsonSerializerOptions options)
   {
      writer.WriteStartObject();

      foreach (var featurePair in value)
      {
         var featureType = featurePair.Key;
         var feature = featurePair.Value;

         if (feature != null)
         {
            writer.WritePropertyName(featureType.FullName!);
            JsonSerializer.Serialize(writer, feature, featureType, options);
         }
      }

      writer.WriteEndObject();
   }
}

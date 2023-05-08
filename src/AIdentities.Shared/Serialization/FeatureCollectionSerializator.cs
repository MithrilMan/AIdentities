using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Features;

namespace AIdentities.Shared.Serialization;

/// <summary>
/// A custom JSON converter for the <see cref="FeatureCollection"/> class.
/// This is meant to be used as a long lived instance (singleton) because makes use 
/// of caching to improve the performance.
/// </summary>
public class FeatureCollectionJsonConverter : JsonConverter<FeatureCollection>
{
   /// <summary>
   /// This class is used to serialize the features of an AIdentity that can't be resolved during the reading.
   /// This way the features can be saved and restored without losing data.
   /// </summary>
   /// <param name="Data"></param>
   public class UnresolvedFeatures : Dictionary<string, Dictionary<string, object>> { }

   readonly Dictionary<string, Type> _knownFeatures;

   readonly ILogger _logger;

   public FeatureCollectionJsonConverter(ILogger logger, IEnumerable<Type> registeredFeatures)
   {
      _logger = logger;
      _knownFeatures = registeredFeatures.ToDictionary(r => r.FullName!, r => r);
   }

   public override FeatureCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
   {
      if (reader.TokenType != JsonTokenType.StartObject)
      {
         throw new JsonException("Expected a JSON object.");
      }

      var featureCollection = new FeatureCollection();
      var unresolvedFeatures = new UnresolvedFeatures();

      while (reader.Read())
      {
         if (reader.TokenType == JsonTokenType.EndObject)
         {
            // Resolve any remaining unresolved features before returning the FeatureCollection
            var unresolvedExistingFeatures = ResolveUnresolvedFeatures(featureCollection, options);

            // If we didn't had any unresolvedFeatures (happy scenario) return the featureCollection
            if (unresolvedFeatures.Count == 0) return featureCollection;

            MergeUnresolvedFeatures(
               featureCollection: featureCollection,
               existingUnresolvedFeatures: unresolvedExistingFeatures,
               unresolvedFeatures: unresolvedFeatures);

            return featureCollection;
         }

         if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException("Expected a JSON property name.");

         // Read the next feature from the JSON input and add it to the FeatureCollection or the UnresolvedFeatures collection
         ReadFeature(ref reader, featureCollection, unresolvedFeatures, options);
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

   private void ReadFeature(ref Utf8JsonReader reader,
                            FeatureCollection featureCollection,
                            UnresolvedFeatures unresolvedFeatures,
                            JsonSerializerOptions options)
   {
      string featureTypeName = reader.GetString()!;

      // Find the Type corresponding to the feature name
      Type? featureType = FindFeatureType(featureTypeName!);

      if (featureType != null)
      {
         // Deserialize the feature and add it to the FeatureCollection
         featureCollection[featureType] = JsonSerializer.Deserialize(ref reader, featureType, options);
      }
      else
      {
         // If the feature Type could not be found, add it to the UnresolvedFeatures collection
         _logger.LogWarning("Unable to resolve the feature type '{featureTypeName}', adding it to UnresolvedFeatures.", featureTypeName);
         unresolvedFeatures.Add(featureTypeName!, JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader, options)!);
      }
   }

   /// <summary>
   /// Tries to resolve the feature type.
   /// </summary>
   /// <param name="featureTypeName">The feature <see cref="Type.FullName"/> </param>
   /// <returns>The resolved type, or null if not found.</returns>
   private Type? FindFeatureType(string featureTypeName)
   {
      // Look for the feature Type in the current assembly or the _knownFeatures dictionary
      Type? featureType = Type.GetType(featureTypeName);

      // If not found, search in known features
      if (featureType == null)
      {
         _knownFeatures.TryGetValue(featureTypeName, out featureType);
      }

      return featureType;
   }


   /// <summary>
   /// Before returning the featureCollection, check if the json have any unresolvedFeatures that can be resolved now.
   /// </summary>
   /// <param name="reader"></param>
   /// <param name="featureCollection">The feature collection we can try to fix if we resolve some previously unresolved feature.</param>
   /// <param name="options">Serialization options.</param>
   /// <returns>
   /// Remaining unresolved features that we tried to resolve but couldn't.
   /// </returns>
   private UnresolvedFeatures? ResolveUnresolvedFeatures(FeatureCollection featureCollection, JsonSerializerOptions options)
   {
      var existingUnresolvedFeatures = featureCollection.Get<UnresolvedFeatures>();
      if (existingUnresolvedFeatures == null) return null;

      var unresolvedFeaturesToRemove = new List<string>();
      foreach (var feature in existingUnresolvedFeatures)
      {
         var featureName = feature.Key;
         var featureToFix = FindFeatureType(featureName);

         // no luck, can't resolve the type, let it leave as an UnresolvedFeature
         if (featureToFix == null) continue;

         // yay, we resolved the type, we can restore the feature now, remove it from the UnresolvedFeature
         // after we convert the unresolved value to the new known type.
         var serializedOriginalValue = JsonSerializer.Serialize(feature.Value, options);
         featureCollection[featureToFix] = JsonSerializer.Deserialize(serializedOriginalValue, featureToFix, options);
         unresolvedFeaturesToRemove.Add(featureName);
      }

      // Remove the resolved features from existingUnresolvedFeatures
      foreach (var unresolvedFeatureToRemove in unresolvedFeaturesToRemove)
      {
         existingUnresolvedFeatures.Remove(unresolvedFeatureToRemove);
      }

      // if we don't have any other unresolved features, remove it from the FeatureCollection
      if (existingUnresolvedFeatures.Count == 0)
      {
         featureCollection.Set<UnresolvedFeatures>(null);
         return null;
      }

      return existingUnresolvedFeatures;
   }



   /// <summary>
   /// Merges the unresolvedFeatures with eventual existing unresolved features and ensures
   /// they are stored into the featureCollection.
   /// </summary>
   /// <param name="featureCollection"></param>
   /// <param name="existingUnresolvedFeatures"></param>
   /// <param name="unresolvedFeatures"></param>
   private static void MergeUnresolvedFeatures(FeatureCollection featureCollection,
                                               UnresolvedFeatures? existingUnresolvedFeatures,
                                               UnresolvedFeatures unresolvedFeatures)
   {
      // If the feature didn't already had an UnresolvedFeatures feature, assing the new one
      if (existingUnresolvedFeatures is null or { Count: 0 })
      {
         featureCollection.Set(unresolvedFeatures);
         return;
      }

      // If the feature already had stored an UnresolvedFeatures feature, merge unresolvedFeature to it.
      // Warning: this may override previous UnresolvedFeatures items with the same key
      foreach (var unresolvedFeature in unresolvedFeatures)
      {
         existingUnresolvedFeatures[unresolvedFeature.Key] = unresolvedFeature.Value;
      }

      // If the feature already had stored an UnresolvedFeatures feature, merge unresolvedFeature to it.
      // Warning: this may override previous UnresolvedFeatures items with the same key
      foreach (var unresolvedFeature in unresolvedFeatures)
      {
         existingUnresolvedFeatures[unresolvedFeature.Key] = unresolvedFeature.Value;
      }
   }
}

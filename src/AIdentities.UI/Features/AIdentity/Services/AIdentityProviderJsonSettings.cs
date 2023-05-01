using System.Text.Json;

namespace AIdentities.UI.Features.AIdentity.Services;

public class AIdentityProviderSerializationSettings
{
   public JsonSerializerOptions SerializerOptions { get; } = new JsonSerializerOptions(JsonSerializerOptions.Default);

   public AIdentityProviderSerializationSettings()
   {
      SerializerOptions.Converters.Add(new AIdentities.Shared.Serialization.FeatureCollectionJsonConverter());
   }
}

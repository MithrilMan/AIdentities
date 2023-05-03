using System.Text.Json;

namespace AIdentities.UI.Features.AIdentityManagement.Services;

public class AIdentityProviderSerializationSettings
{
   public JsonSerializerOptions SerializerOptions { get; } = new JsonSerializerOptions(JsonSerializerOptions.Default);

   public AIdentityProviderSerializationSettings()
   {
      SerializerOptions.Converters.Add(new Shared.Serialization.FeatureCollectionJsonConverter());
   }
}

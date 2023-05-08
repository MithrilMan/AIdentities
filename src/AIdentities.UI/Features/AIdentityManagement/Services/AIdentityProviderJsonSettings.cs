using System.Text.Json;

namespace AIdentities.UI.Features.AIdentityManagement.Services;

public class AIdentityProviderSerializationSettings
{
   readonly ILogger<AIdentityProviderSerializationSettings> _logger;

   public JsonSerializerOptions SerializerOptions { get; } = new JsonSerializerOptions(JsonSerializerOptions.Default);

   public AIdentityProviderSerializationSettings(ILogger<AIdentityProviderSerializationSettings> logger, IEnumerable<AIdentityFeatureRegistration> aIdentityFeatureRegistrations)
   {
      _logger = logger;

      InitializeSerializerOptions(aIdentityFeatureRegistrations);
   }

   private void InitializeSerializerOptions(IEnumerable<AIdentityFeatureRegistration> aIdentityFeatureRegistrations)
   {
      var registeredFeatures = aIdentityFeatureRegistrations.Select(x => x.FeatureType);
      SerializerOptions.Converters.Add(new Shared.Serialization.FeatureCollectionJsonConverter(_logger, registeredFeatures));
   }
}

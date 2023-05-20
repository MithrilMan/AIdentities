namespace AIdentities.Shared.Features.AIdentities.Services;

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
      SerializerOptions.Converters.Add(new Serialization.FeatureCollectionJsonConverter(_logger, registeredFeatures));
   }
}

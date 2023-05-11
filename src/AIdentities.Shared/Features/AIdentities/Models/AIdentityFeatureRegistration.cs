namespace AIdentities.Shared.Features.AIdentities.Models;

/// <summary>
/// Holds registration about an AIdentity feature Type and it's UI Type.
/// </summary>
/// <param name="FeatureType">The feature Type.</param>
/// <param name="FeatureTabUIType">The feature UI Type.</param>
/// <param name="UITitle">The UI title that will be shown on the Tab Panel.</param>
public record AIdentityFeatureRegistration(Type FeatureType, Type FeatureTabUIType, string UITitle);

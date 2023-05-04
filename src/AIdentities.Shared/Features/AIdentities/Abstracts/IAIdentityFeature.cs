namespace AIdentities.Shared.Features.AIdentities.Abstracts;

/// <summary>
/// This is a marker interface used to identify an AIdentity feature.
/// Plugin developers should implement this interface on their plugins if they want to add contextual information
/// to an AIdentity.
/// Everything could be stored in the AIdentity's Features property, but this interface is used to expose a form
/// within the AIdentity's management page, to let the user manage the feature.
/// </summary>
public interface IAIdentityFeature { }

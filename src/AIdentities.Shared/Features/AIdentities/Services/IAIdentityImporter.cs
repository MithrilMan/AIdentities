namespace AIdentities.Shared.Features.AIdentities.Services;

/// <summary>
/// An interface for importing characters from a file.
/// This could be used from plugin developers to implement custom importers for AIdentitie's character files.
/// </summary>
public interface IAIdentityImporter
{
   /// <summary>
   /// Imports a <see cref="AIdentity"/> from a file.
   /// </summary>
   /// <param name="aIdentityPath">The file path to import the <see cref="AIdentity"/> from.</param>
   /// <returns>The imported <see cref="AIdentity"/>.</returns>
   AIdentity? ImportAIdentity(string aIdentityPath);
}

using Microsoft.AspNetCore.Components.Forms;

namespace AIdentities.Shared.Features.AIdentities.Services;

/// <summary>
/// An interface for importing characters from a file.
/// This could be used from plugin developers to implement custom importers for AIdentitie's character files.
/// </summary>
public interface IAIdentityImporter
{
   /// <summary>
   /// The name of the importer.
   /// </summary>
   string Name { get; }

   /// <summary>
   /// A description of the importer.
   /// </summary>
   string Description { get; }

   /// <summary>
   /// The file extensions that the importer supports.
   /// </summary>
   IEnumerable<string> AllowedFileExtensions { get; }

   /// <summary>
   /// Imports a <see cref="AIdentity"/> from a file.
   /// </summary>
   /// <param name="aIdentityFile">The file to import the <see cref="AIdentity"/> from.</param>
   /// <returns>The imported <see cref="AIdentity"/>.</returns>
   Task<AIdentity?> ImportAIdentity(IBrowserFile aIdentityFile);
}

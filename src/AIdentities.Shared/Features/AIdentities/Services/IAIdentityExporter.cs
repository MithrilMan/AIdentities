namespace AIdentities.Shared.Features.AIdentities.Services;

/// <summary>
/// An interface for exporting characters from a file.
/// This could be used from plugin developers to implement custom exporters for AIdentitie's character files.
/// </summary>
public interface IAIdentityExporter
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
   /// Exports a <see cref="AIdentity"/> to a file.
   /// </summary>
   /// <param name="aIdentity">The <see cref="AIdentity"/> to export.</param>
   /// <param name="fileName">The file name of the exported <see cref="AIdentity"/>.</param>
   Task ExportAIdentityAsync(AIdentity aIdentity, string fileName);
}

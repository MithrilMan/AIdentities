namespace AIdentities.Shared.Features.AIdentities.Services;

/// <summary>
/// An interface for exporting characters from a file.
/// This could be used from plugin developers to implement custom exporters for AIdentitie's character files.
/// </summary>
interface IAIdentityExporter
{
   /// <summary>
   /// Exports a <see cref="AIdentity"/> to a file.
   /// </summary>
   /// <param name="aIdentity">The <see cref="AIdentity"/> to export.</param>
   /// <param name="exportedFilePath">The file path to export the <see cref="AIdentity"/> to.</param>
   Task ExportAIdentityAsync(AIdentity aIdentity, string exportedFilePath);
}

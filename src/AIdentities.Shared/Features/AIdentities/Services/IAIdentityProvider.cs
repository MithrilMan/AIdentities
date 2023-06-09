﻿namespace AIdentities.Shared.Features.AIdentities.Services;

/// <summary>
/// A service that provides access to <see cref="AIdentity"/>s.
/// </summary>
public interface IAIdentityProvider
{
   /// <summary>
   /// Gets all <see cref="AIdentity"/>s.
   /// </summary>
   /// <returns>All <see cref="AIdentity"/>s.</returns>
   IEnumerable<AIdentity> All();

   /// <summary>
   /// Gets a <see cref="AIdentity"/> by its Id.
   /// </summary>
   /// <param name="id">The Id of the <see cref="AIdentity"/> to get.</param>
   /// <returns>The <see cref="AIdentity"/> with the given Id, or null if not found.</returns>
   AIdentity? Get(Guid id);

   /// <summary>
   /// Gets a <see cref="AIdentity"/> by its name.
   /// Name isn't unique, so this method returns the first <see cref="AIdentity"/> with the given name.
   /// </summary>
   /// <param name="id">The Id of the <see cref="AIdentity"/> to get.</param>
   /// <returns>The first <see cref="AIdentity"/> with the given name, or null if not found.</returns>
   AIdentity? Get(string name);

   /// <summary>
   /// Creates a new <see cref="AIdentity"/>.
   /// </summary>
   /// <param name="newAIdentity">The <see cref="AIdentity"/> to create.</param>
   /// <returns>True if the <see cref="AIdentity"/> was created, otherwise false.</returns>
   bool Create(AIdentity newAIdentity);

   /// <summary>
   /// Updates an existing <see cref="AIdentity"/>.
   /// </summary>
   /// <param name="updatedAIdentity">The <see cref="AIdentity"/> to update.</param>
   /// <returns>True if the <see cref="AIdentity"/> was updated, otherwise false.</returns>
   bool Update(AIdentity updatedAIdentity);

   /// <summary>
   /// Deletes an existing <see cref="AIdentity"/>.
   /// </summary>
   /// <param name="deletedAIdentity">The <see cref="AIdentity"/> to delete.</param>
   /// <returns>True if the <see cref="AIdentity"/> was deleted, otherwise false.</returns>
   bool Delete(AIdentity deletedAIdentity);

   /// <summary>
   /// Gets the raw content of a <see cref="AIdentity"/> file.
   /// </summary>
   /// <param name="id">The Id of the <see cref="AIdentity"/> to download.</param>
   /// <returns>The original file name and the content of the <see cref="AIdentity"/> file.</returns>
   Task<(string originalFileName, byte[] content)> GetRaw(Guid id);
}

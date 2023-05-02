namespace AIdentities.Chat.Extendability;

/// <summary>
/// An interface for importing characters from a file.
/// This could be used from plugin developers to implement custom importers for AIdentitie's character files.
/// </summary>
public interface IAIdentityImporter
{
   AIdentity ImportCharacter(string characterPath);
}

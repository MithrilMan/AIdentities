namespace AIdentities.BooruAIdentityImporter.Services.FormatDetector;
public interface ICharacterFormatDetector
{
   string Format { get; }
   bool IsValid(Dictionary<string, object> decodeAsDictionary);
}

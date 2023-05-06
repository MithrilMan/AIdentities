namespace AIdentities.BooruAIdentityImporter.Services.FormatDetector;
public abstract class BaseCharacterFormat : ICharacterFormatDetector
{
   public abstract string Format { get; }
   protected abstract string[] Properties { get; }

   public virtual bool IsValid(Dictionary<string, object> decodeAsDictionary)
   {
      return Properties.All(property => decodeAsDictionary.ContainsKey(property));
   }
}

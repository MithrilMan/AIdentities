namespace AIdentities.BooruAIdentityImporter.Services.FormatDetector;

public class FormatDetector : IFormatDetector
{
   readonly ILogger<FormatDetector> _logger;
   readonly IEnumerable<ICharacterFormatDetector> _formats;

   public FormatDetector(ILogger<FormatDetector> logger, IEnumerable<ICharacterFormatDetector> formats)
   {
      _logger = logger;
      _formats = formats;
   }

   public IEnumerable<string> DetectFormat(Dictionary<string, object>? decodeAsDictionary)
   {
      if (decodeAsDictionary == null) throw new ArgumentNullException(nameof(decodeAsDictionary));

      foreach (var format in _formats)
      {
         if (format.IsValid(decodeAsDictionary))
         {
            yield return format.Format;
         }
      }
   }
}

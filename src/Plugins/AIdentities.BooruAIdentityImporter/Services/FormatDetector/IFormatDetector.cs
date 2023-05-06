namespace AIdentities.BooruAIdentityImporter.Services.FormatDetector;

/// <summary>
/// Detects the format of a decoded character file
/// </summary>
public interface IFormatDetector
{
   /// <summary>
   /// Detects one or more formats of a decoded character file.
   /// Since some formats may be a subset of another format, this method returns a list of formats.
   /// </summary>
   /// <param name="data">The decoded character file</param>
   /// <returns>A list of formats that the character file matches.
   /// </returns>
   IEnumerable<string> DetectFormat(Dictionary<string, object>? data);
}

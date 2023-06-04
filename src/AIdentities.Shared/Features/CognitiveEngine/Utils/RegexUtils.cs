using System.Text.RegularExpressions;

namespace AIdentities.Shared.Features.CognitiveEngine.Utils;

public static partial class SkillRegexUtils
{

   /// <summary>
   /// Tries to extract a json block out of a text.
   /// </summary>
   /// <returns></returns>
   [GeneratedRegex("\\{(.|\\s)*\\}", RegexOptions.Multiline)]
   public static partial Regex ExtractJson();

   /// <summary>
   /// Tries to extract the command name out of a text.
   /// </summary>
   /// <returns></returns>
   [GeneratedRegex("(?<=Skill:\\s+)\\w+", RegexOptions.IgnoreCase)]
   public static partial Regex ExtractSkillName();
}

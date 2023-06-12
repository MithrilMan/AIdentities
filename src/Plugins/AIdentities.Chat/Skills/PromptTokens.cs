using System.Text.RegularExpressions;

namespace AIdentities.Chat.Skills;

public static partial class PromptTokens
{
   public const string TOKEN_AIDENTITY_NAME = "{{AIDENTITY_NAME}}";
   public const string TOKEN_AIDENTITY_BACKGROUND = "{{AIDENTITY_BACKGROUND}}";
   public const string TOKEN_AIDENTITY_PERSONALITY = "{{AIDENTITY_PERSONALITY}}";
   public const string TOKEN_EXAMPLE_MESSAGES = "{{EXAMPLE_MESSAGES}}";
   public const string TOKEN_PARTICIPANTS = "{{PARTICIPANTS}}";

   public static List<string> KnownTokens { get; } = new()
   {
      TOKEN_AIDENTITY_NAME,
      TOKEN_AIDENTITY_BACKGROUND,
      TOKEN_AIDENTITY_PERSONALITY,
      TOKEN_EXAMPLE_MESSAGES,
      TOKEN_PARTICIPANTS
   };

   //create a regular expression that matches all tokens in a text. Tokens are surrounded by {{ and }}
   public static Regex TokenRegex { get; } = TokenExtractorRegex();



   [GeneratedRegex("\\{\\{[A-Z_]+\\}\\}")]
   private static partial Regex TokenExtractorRegex();
}

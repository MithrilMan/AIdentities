namespace AIdentities.Chat.Skills;

public static class PromptTokens
{
   public const string TOKEN_AIDENTITY_NAME = "<AIDENTITY_NAME>";
   public const string TOKEN_AIDENTITY_BACKGROUND = "<AIDENTITY_BACKGROUND>";
   public const string TOKEN_AIDENTITY_PERSONALITY = "<AIDENTITY_PERSONALITY>";
   public const string TOKEN_EXAMPLE_MESSAGES = "<EXAMPLE_MESSAGES>";
   public const string TOKEN_PARTICIPANTS = "<PARTICIPANTS>";
   public const string EXAMPLE_MESSAGES_INSTRUCTION = $"This is how {TOKEN_AIDENTITY_NAME} should talk";
}

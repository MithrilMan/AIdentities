namespace AIdentities.Chat.Skills.ReplyToPrompt;

record Args
{
   public static SkillArgumentDefinition ConversationContextDefinition
      = new SkillArgumentDefinition(nameof(ConversationContext), "What's the conversation context", true);

   public string? ConversationContext { get; set; }
}

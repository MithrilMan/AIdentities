using System.Diagnostics.CodeAnalysis;

namespace AIdentities.Chat.Skills.ReplyToPrompt;

[SkillDefinition(
   Name = NAME,
   Description = "The AIdenity has to reply to a prompt",
   Tags = new[] { SkillTags.TAG_CHAT }
   )]
[SkillInputDefinition(
   Name = IN_CONVERSATION_CONTEXT, Type = SkillVariableType.String,
   Description = "What's the conversation context"
   )]
[SkillOutputDefinition(
   Name = OUT_REPLY, Type = SkillVariableType.String,
   Description = "The sentence that the AIdentity replied to the prompt."
   )]
public partial class ReplyToPrompt : Skill
{
   const string NAME = nameof(ReplyToPrompt);
   const string IN_CONVERSATION_CONTEXT = nameof(ConversationContext);
   const string OUT_REPLY = nameof(SetReply);

   public static string? ConversationContext(SkillExecutionContext context)
     => context.GetInput<string>(IN_CONVERSATION_CONTEXT);

   public static void SetReply(SkillExecutionContext context, string reply)
     => context.SetOutput(OUT_REPLY, reply);

   protected override bool ValidateInputs(Prompt prompt, SkillExecutionContext context, [MaybeNullWhen(true)] out InvalidArgumentsThought error)
   {
      string? conversationContext;
      try
      {
         conversationContext = ConversationContext(context);
      }
      catch (Exception ex)
      {
         error = context.InvalidArgumentsThought(ex.Message);
         return false;
      }


      if (conversationContext is null)
      {
         if (TryExtractJson<dynamic>(prompt.Text, out var args))
         {
            context.SetInput(IN_CONVERSATION_CONTEXT, args[IN_CONVERSATION_CONTEXT]);
         }
      }

      error = null;
      return true;
   }
}

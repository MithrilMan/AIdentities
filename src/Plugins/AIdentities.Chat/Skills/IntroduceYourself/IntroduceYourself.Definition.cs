namespace AIdentities.Chat.Skills.IntroduceYourself;

[SkillDefinition(
   Name = NAME,
   Description = "The AIdenity has to introduce itself to the conversation",
   Tags = new[] { SkillTags.TAG_CHAT }
   )]
[SkillOutputDefinition(
   Name = OUT_PRESENTATION, Type = SkillVariableType.String,
   Description = "The LLM generated presentation"
   )]
public partial class IntroduceYourself
{
   const string NAME = nameof(IntroduceYourself);
   const string OUT_PRESENTATION = nameof(SetPresentation);

   private static void SetPresentation(SkillExecutionContext context, string? presentation)
      => context.SetOutput(OUT_PRESENTATION, presentation);
}

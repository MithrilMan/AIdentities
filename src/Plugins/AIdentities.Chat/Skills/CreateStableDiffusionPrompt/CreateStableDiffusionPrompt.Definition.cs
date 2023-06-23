﻿using System.Diagnostics.CodeAnalysis;

namespace AIdentities.Chat.Skills.CreateStableDiffusionPrompt;

[SkillDefinition(
   Name = NAME,
   Description = "Generates a prompt for stable diffusion based on current specifications given in a conversation.",
   Tags = new[] { SkillTags.TAG_CHAT }
   )]
[SkillInputDefinition(
   Name = IN_PROMPT_SPECIFICATIONS, Type = SkillVariableType.String,
   Description = "What are the given specifications."
   )]
[SkillOutputDefinition(
   Name = OUT_GENERATED_PROMPT, Type = SkillVariableType.String,
   Description = "The Stable Diffusion prompt generated by the skill."
   )]
public partial class CreateStableDiffusionPrompt
{
   const string NAME = nameof(CreateStableDiffusionPrompt);
   const string IN_PROMPT_SPECIFICATIONS = nameof(PromptSpecifications);
   const string OUT_GENERATED_PROMPT = nameof(SetGeneratedPrompt);

   public static string? PromptSpecifications(SkillExecutionContext context)
     => context.GetInput<string>(IN_PROMPT_SPECIFICATIONS);

   public static void SetGeneratedPrompt(SkillExecutionContext context, string generatedPrompt)
      => context.SetOutput(OUT_GENERATED_PROMPT, generatedPrompt);

   protected override bool ValidateInputs(Prompt prompt, SkillExecutionContext context, [MaybeNullWhen(true)] out InvalidArgumentsThought error)
   {
      string? conversationContext;
      try
      {
         conversationContext = PromptSpecifications(context);
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
            context.SetInput(IN_PROMPT_SPECIFICATIONS, args[IN_PROMPT_SPECIFICATIONS]);
         }
      }

      error = null;
      return true;
   }
}
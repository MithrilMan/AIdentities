using System.Text;
using AIdentities.Shared.Features.CognitiveEngine.Prompts;

namespace AIdentities.Shared.Features.CognitiveEngine.Engines.Mithril;
public static class PromptTemplates
{
   const string TOKEN_USER_PROMPT = $"{nameof(TOKEN_USER_PROMPT)}";
   const string TOKEN_SKILL_ARGUMENTS = $"{nameof(TOKEN_SKILL_ARGUMENTS)}";
   const string TOKEN_DETECTED_SKILL = $"{nameof(TOKEN_DETECTED_SKILL)}";
   const string TOKEN_DETECTED_SKILL_EXAMPLE = $"{nameof(TOKEN_DETECTED_SKILL_EXAMPLE)}";
   const string TOKEN_AVAILABLE_SKILLS = $"{nameof(TOKEN_AVAILABLE_SKILLS)}";

   /// <summary>
   /// Prompt to detect a skill within the user prompt.
   /// Substitutes these tokens:
   /// <see cref="TOKEN_AVAILABLE_SKILLS"/> with the actual available skills.
   /// <see cref="TOKEN_USER_PROMPT"/> with the actual user prompt.
   /// </summary>
   const string FIND_SKILL = $"""
      Here a list of available skills that you can use, in yaml format:
      ```yaml
      skills:
      {TOKEN_AVAILABLE_SKILLS}
      ```

      Find the skill that best matches the user prompt.
      You have to return just the skill name that matches better the user prompt.
      If you can't find any skill, just return the word "DUNNO".

      <START Examples>
      UserRequest: I don't like the color of the background, I'd like it to be blue
      Reasoning : The user is asking to change the theme of the application, ChangeTheme satisfies the request.
      Skill: ChangeTheme

      UserRequest: I want to know the weather in Rome
      Reasoning : The user is asking for the weather in Rome, no available skill satisfy the request.
      Skill: DUNNO

      UserRequest: I'd like a colorful theme for the application
      Reasoning : The user is asking to change the theme of the application, ChangeTheme satisfies the request.
      Skill: ChangeTheme
      <END Examples>

      UserRequest: {TOKEN_USER_PROMPT}
      Reasoning:

      """;

   /// <summary>
   /// Prompt to receive a JSON formatted response that can be feed into the detected skill.
   /// Substitutes these tokens:
   /// <see cref="TOKEN_DETECTED_SKILL"/> with the actual detected skill.
   /// <see cref="TOKEN_SKILL_ARGUMENTS"/> with the actual skill arguments.
   /// <see cref="TOKEN_DETECTED_SKILL_EXAMPLE"/> with the actual detected skill example.
   /// <see cref="TOKEN_USER_PROMPT"/> with the actual user prompt.
   /// </summary>
   const string SKILL_EXTRACTION_WITH_PARAMETERS = $"""
      Here the yaml definition of the skill that you have to execute
      ```yaml
      Skill: {TOKEN_DETECTED_SKILL}
        arguments:
      {TOKEN_SKILL_ARGUMENTS}
      ```
      
      Based on the user prompt, think step to step and then extract the arguments that the user has passed to the skill and create a JSON object with the arguments. 
      
      <START Examples>
      {TOKEN_DETECTED_SKILL_EXAMPLE}
      <END Examples>
      
      UserRequest: {TOKEN_USER_PROMPT}
      Reasoning: 

      """;


   const string SKILL_EXTRACTION_WITHOUT_PARAMETERS = $"""
      Here the yaml definition of the skill that you have to execute
      ```yaml
      Skill: {TOKEN_DETECTED_SKILL}
        arguments:
      {TOKEN_SKILL_ARGUMENTS}
      ```
      
      Based on the user prompt, explain in the context of the {TOKEN_DETECTED_SKILL} command what the user wants to do.
      
      <START Examples>
      UserRequest: I don't like the color of the background, I'd like it to be blue
      Reasoning: The user is asking to change the background of the theme with a blue color.
      Request: Change the application theme background color to blue.
      
      UserRequest: I'd like a colorful theme for the application
      Reasoning: The user is asking to change the theme of the application with a colorful one.
      Request: Change the application theme to a colorful one
      <END Examples>
      
      UserRequest: {TOKEN_USER_PROMPT}
      Reasoning:  

      """;

   public static string BuildFindSkillPrompt(Prompt userPrompt, IEnumerable<ISkill> availableSkills)
   {
      var sbAvailableSkills = new StringBuilder();
      foreach (var skillAction in availableSkills)
      {
         sbAvailableSkills.AppendLine($"  - skill: {skillAction.Name}");
         sbAvailableSkills.AppendLine($"    when: {skillAction.ActivationContext}");
      }

      var sb = new StringBuilder(FIND_SKILL);
      sb.Replace(TOKEN_USER_PROMPT, userPrompt.Text);
      sb.Replace(TOKEN_AVAILABLE_SKILLS, string.Join(Environment.NewLine, sbAvailableSkills.ToString()));
      return sb.ToString();
   }

   public static string BuildGenerateSkillParametersJson(Prompt userPrompt, ISkill detectedSkillAction)
   {
      var sbSkillArgs = new StringBuilder();
      foreach (var arg in detectedSkillAction.Arguments)
      {
         sbSkillArgs.AppendLine($"  - name: {arg.Name}");
         sbSkillArgs.AppendLine($"    description: {arg.Description}");
         sbSkillArgs.AppendLine($"    required: {arg.IsRequired}");
      }

      StringBuilder sb = new();
      if (detectedSkillAction.Arguments.Any())
      {
         sb.Append(SKILL_EXTRACTION_WITH_PARAMETERS);
      }
      else
      {
         sb.Append(SKILL_EXTRACTION_WITHOUT_PARAMETERS);
      }

      sb.Replace(TOKEN_DETECTED_SKILL, userPrompt.Text);
      sb.Replace(TOKEN_SKILL_ARGUMENTS, string.Join(Environment.NewLine, sbSkillArgs.ToString()));
      sb.Replace(TOKEN_DETECTED_SKILL_EXAMPLE, string.Join(Environment.NewLine, sbSkillArgs.ToString()));
      sb.Replace(TOKEN_DETECTED_SKILL_EXAMPLE, string.Join(Environment.NewLine, sbSkillArgs.ToString()));
      return sb.ToString();
   }
}

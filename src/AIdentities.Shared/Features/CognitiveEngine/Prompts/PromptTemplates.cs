using System.Text;

namespace AIdentities.Shared.Features.CognitiveEngine.Prompts;

public static class PromptTemplates
{
   public const string UNKNOWN_SKILL = "DUNNO";
   const string TOKEN_USER_PROMPT = $"<<{nameof(TOKEN_USER_PROMPT)}>>";
   const string TOKEN_SKILL_ARGUMENTS = $"<<{nameof(TOKEN_SKILL_ARGUMENTS)}>>";
   const string TOKEN_DETECTED_SKILL = $"<<{nameof(TOKEN_DETECTED_SKILL)}>>";
   const string TOKEN_DETECTED_SKILL_EXAMPLE = $"<<{nameof(TOKEN_DETECTED_SKILL_EXAMPLE)}>>";
   const string TOKEN_AVAILABLE_SKILLS = $"<<{nameof(TOKEN_AVAILABLE_SKILLS)}>>";
   const string TOKEN_GOOD_SKILL_DETECTOR_EXAMPLE1 = $"<<{nameof(TOKEN_GOOD_SKILL_DETECTOR_EXAMPLE1)}>>";
   const string TOKEN_GOOD_SKILL_DETECTOR_EXAMPLE2 = $"<<{nameof(TOKEN_GOOD_SKILL_DETECTOR_EXAMPLE2)}>>";

   /// <summary>
   /// Prompt to detect a skill within the user prompt.
   /// Substitutes these tokens:
   /// <see cref="TOKEN_AVAILABLE_SKILLS"/> with the actual available skills.
   /// <see cref="TOKEN_USER_PROMPT"/> with the actual user prompt.
   /// <see cref="TOKEN_GOOD_SKILL_DETECTOR_EXAMPLE1"/> with the actual good skill detector example 1.
   /// <see cref="TOKEN_GOOD_SKILL_DETECTOR_EXAMPLE2"/> with the actual good skill detector example 2.
   /// </summary>
   const string FIND_SKILL = $"""
      Here a list of available skills that you can use, in yaml format:
      ```yaml
      skills:
      {TOKEN_AVAILABLE_SKILLS}
      ```

      Find the skill that best matches the user prompt.
      You have to return just the skill name that matches better the user prompt.
      If you can't find any skill, just return the word "{UNKNOWN_SKILL}".

      <START Examples>
      {TOKEN_GOOD_SKILL_DETECTOR_EXAMPLE1}

      UserRequest: I want to know the weather in Rome
      Reasoning : The user is asking for the weather in Rome, no available skill satisfy the request.
      Skill: {UNKNOWN_SKILL}

      {TOKEN_GOOD_SKILL_DETECTOR_EXAMPLE2}
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

   public static string BuildFindSkillPrompt(Prompt userPrompt, IEnumerable<SkillDefinition> availableSkills)
   {
      var sbAvailableSkills = new StringBuilder();
      foreach (var skill in availableSkills)
      {
         sbAvailableSkills.AppendLine($"  - skill: {skill.Name}");
         sbAvailableSkills.AppendLine($"    when: {skill.Description}");
      }

      var rnd = new Random();
      var examples = (
         from skill in availableSkills
         from example in skill.Examples
         where example.IsStandardExample
         select new
         {
            Skill = skill,
            Example = example
         }).ToList();


      var sb = new StringBuilder(FIND_SKILL);
      sb.Replace(TOKEN_USER_PROMPT, userPrompt.Text);
      sb.Replace(TOKEN_AVAILABLE_SKILLS, string.Join(Environment.NewLine, sbAvailableSkills.ToString()));

      var goodExample = examples.First();
      sb.Replace(TOKEN_GOOD_SKILL_DETECTOR_EXAMPLE1, $"""
                                                     UserRequest: {goodExample.Example.UserRequest}
                                                     Reasoning : {goodExample.Example.Reasoning}
                                                     Skill: {goodExample.Skill.Name}
                                                     """);

      if (examples.Count > 1)
      {
         goodExample = examples.Skip(1).ElementAt(rnd.Next(1, examples.Count - 1));
      }
      sb.Replace(TOKEN_GOOD_SKILL_DETECTOR_EXAMPLE2, $"""
                                                     UserRequest: {goodExample.Example.UserRequest}
                                                     Reasoning : {goodExample.Example.Reasoning}
                                                     Skill: {goodExample.Skill.Name}
                                                     """);
      return sb.ToString();
   }

   public static string BuildGenerateSkillParametersJson(Prompt userPrompt, SkillDefinition detectedSkill)
   {
      var sbSkillArgs = new StringBuilder();
      foreach (var arg in detectedSkill.Inputs)
      {
         sbSkillArgs.AppendLine($"  - name: {arg.Name}");
         sbSkillArgs.AppendLine($"    description: {arg.Description}");
         sbSkillArgs.AppendLine($"    required: {arg.IsRequired}");
      }

      StringBuilder sb = new();
      if (detectedSkill.Inputs.Any())
      {
         sb.Append(SKILL_EXTRACTION_WITH_PARAMETERS);
      }
      else
      {
         sb.Append(SKILL_EXTRACTION_WITHOUT_PARAMETERS);
      }

      sb.Replace(TOKEN_DETECTED_SKILL, detectedSkill.Name);
      sb.Replace(TOKEN_SKILL_ARGUMENTS, string.Join(Environment.NewLine, sbSkillArgs.ToString()));
      sb.Replace(TOKEN_DETECTED_SKILL_EXAMPLE, string.Join(
         Environment.NewLine,
         detectedSkill.Examples.Select(e =>
            $"""
            UserRequest: {e.UserRequest}
            Reasoning: {e.Reasoning}
            JSON: {e.JsonExample}

            """)
         ));
      sb.Replace(TOKEN_USER_PROMPT, string.Join(Environment.NewLine, userPrompt.Text));
      return sb.ToString();
   }
}

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace AIdentities.Chat.Skills.CallAFriend;

//internal static class PromptTemplates
//{
//   const string TOKEN_AIDENTITY_NAMES = $"{nameof(TOKEN_AIDENTITY_NAMES)}";

//   /// <summary>
//   /// Prompt to detect a skill within the user prompt.
//   /// Substitutes these tokens:
//   /// <see cref="TOKEN_AVAILABLE_SKILLS"/> with the actual available skills.
//   /// <see cref="TOKEN_USER_PROMPT"/> with the actual user prompt.
//   /// </summary>
//   const string FIND_AIDENTITY = $"""
//      Giving a list of AIdentitties names, find the correct one:
//      ```yaml
//      names:
//      {TOKEN_AIDENTITY_NAMES}
//      ```

//      Find the AIdentitties names that best matches the user prompt.
//      You have to return just the name that matches better.
//      If you can't find any skill, just return the word "DUNNO".

//      <START Examples>
//      UserRequest: I want to talk with Ciccio Pasticcio
//      Reasoning : The user wants to talk to Ciccio Pasticcio
//      Who: Ciccio Pasticcio

//      UserRequest: I want to know the weather in Rome
//      Reasoning : The user is asking for the weather in Rome, no available skill satisfy the request.
//      skill: DUNNO

//      UserRequest: I'd like a colorful theme for the application
//      Reasoning : The user is asking to change the theme of the application, ChangeTheme satisfies the request.
//      skill: ChangeTheme
//      <END Examples>

//      UserRequest: {TOKEN_USER_PROMPT}
//      Reasoning:

//      """;

//   /// <summary>
//   /// Prompt to receive a JSON formatted response that can be feed into the detected skill.
//   /// Substitutes these tokens:
//   /// <see cref="TOKEN_DETECTED_SKILL"/> with the actual detected skill.
//   /// <see cref="TOKEN_SKILL_ARGUMENTS"/> with the actual skill arguments.
//   /// <see cref="TOKEN_DETECTED_SKILL_EXAMPLE"/> with the actual detected skill example.
//   /// <see cref="TOKEN_USER_PROMPT"/> with the actual user prompt.
//   /// </summary>
//   const string SKILL_EXTRACTION = $"""
//      Here the yaml definition of the skill that you have to execute
//      ```yaml
//      skill: {TOKEN_DETECTED_SKILL}
//        arguments:
//      {TOKEN_SKILL_ARGUMENTS}
//      ```
      
//      Based on the user prompt, think step to step and then extract the arguments that the user has passed to the skill and create a JSON object with the arguments. 
      
//      <START Examples>
//      {TOKEN_DETECTED_SKILL_EXAMPLE}
//      <END Examples>
      
//      UserRequest: {TOKEN_USER_PROMPT}
//      Reasoning: 

//      """;

//   public static string GetFindSkillPrompt(string userPrompt, IEnumerable<string> availableSkills)
//   {
//      var sbAvailableSkills = new StringBuilder();
//      foreach (var command in _brainButlerCommandManager.AvailableCommands)
//      {
//         sb.AppendLine($"  - command: {command.Name}");
//         sb.AppendLine($"    when: {command.ActivationContext}");
//      }

//      var sb = new StringBuilder(FIND_SKILL);
//      sb.Replace(TOKEN_USER_PROMPT, userPrompt);
//      sb.Replace(TOKEN_AVAILABLE_SKILLS, string.Join(Environment.NewLine, availableSkills.Select(s => $"  - {s}")));
//      return sb.ToString();
//   }
//}

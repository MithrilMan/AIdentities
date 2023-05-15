using System.Text;

namespace AIdentities.BrainButler.Services;
public class PromptGenerator : IPromptGenerator
{
   readonly ILogger<PromptGenerator> _logger;
   readonly IBrainButlerCommandManager _brainButlerCommandManager;

   public PromptGenerator(ILogger<PromptGenerator> logger, IBrainButlerCommandManager brainButlerCommandManager)
   {
      _logger = logger;
      _brainButlerCommandManager = brainButlerCommandManager;
   }

   public string GenerateFindCommandPrompt(string userPrompt)
   {
      var sb = new StringBuilder($"""
         Here a list of available commands that you can use, in yaml format:
         ```yaml
         commands:

         """);

      foreach (var command in _brainButlerCommandManager.AvailableCommands)
      {
         sb.AppendLine($"  - command: {command.Name}");
         sb.AppendLine($"    when: {command.ActivationContext}");
      }

      sb.Append("""
         ```

         Find the command that best matches the user prompt.
         You have to return just the command name that matches better the user prompt.
         If you can't find any command, just return the word "DUNNO".

         <START Examples>
         UserRequest: I don't like the color of the background, I'd like it to be blue
         Command: ChangeTheme

         UserRequest: I want to know the weather in Rome
         Command: DUNNO

         UserRequest: I'd like a colorful theme for the application
         Command: ChangeTheme
         <END Examples>

         """);
      sb.AppendLine($"UserRequest: {userPrompt}");
      sb.AppendLine("Command: ");

      return sb.ToString();
   }

   public string? GenerateCommandExtraction(string aiDetectedCommand, string userPrompt, out IBrainButlerCommand? detectedCommand)
   {
      detectedCommand = _brainButlerCommandManager.AvailableCommands.FirstOrDefault(c => c.Name == aiDetectedCommand);
      if (detectedCommand == null) return null;

      var sb = new StringBuilder($"""
         Here the yaml definition of the command that you have to execute
         ```yaml
         command: {aiDetectedCommand}
           arguments:
      
         """);

      if (detectedCommand.Arguments.Count() > 0)
      {
         foreach (var argument in detectedCommand.Arguments)
         {
            sb.AppendLine($"  - name: {argument.Name}");
            sb.AppendLine($"    description: {argument.Description}");
            sb.AppendLine($"    required: {argument.IsRequired}");
         }
         sb.Append($"""
         ```

         Based on the user prompt, extract the arguments that the user has passed to the command and create a JSON object with the arguments. 

         <START Examples>
         {detectedCommand.Examples}
         <END Examples>
         
         UserRequest: {userPrompt}
         JSON: 
         """);
      }
      else
      {
         sb.Append($"""
         ```

         Based on the user prompt, explain in the context of the {aiDetectedCommand} command what the user wants to do.

         <START Examples>
         UserRequest: I don't like the color of the background, I'd like it to be blue
         Request: Change the application theme background color to blue

         UserRequest: I'd like a colorful theme for the application
         Request: Change the application theme to a colorful one
         <END Examples>

         UserRequest: {userPrompt}
         Request: 
         """);
      }

      return sb.ToString();
   }
}

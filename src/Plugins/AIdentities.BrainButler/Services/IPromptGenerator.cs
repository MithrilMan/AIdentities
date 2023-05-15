namespace AIdentities.BrainButler.Services;

public interface IPromptGenerator
{
   /// <summary>
   /// Returns the prompt to find the command that best matches the user prompt.
   /// </summary>
   /// <param name="userPrompt"></param>
   /// <returns></returns>
   string GenerateFindCommandPrompt(string userPrompt);

   /// <summary>
   /// Returns the prompt to pass to the detected command.
   /// If no command is detected, returns null.
   /// </summary>
   /// <param name="aiDetectedCommand">The command detected by the AI.</param>
   /// <param name="userPrompt">The original user prompt.</param>
   /// <returns>The prompt to pass to the detected command.</returns>
   string? GenerateCommandExtraction(string aiDetectedCommand, string userPrompt, out IBrainButlerCommand? detectedCommand);
}

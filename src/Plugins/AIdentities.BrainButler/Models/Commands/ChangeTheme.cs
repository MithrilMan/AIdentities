using System.Text;
using System.Text.Json;

namespace AIdentities.BrainButler.Models.Commands;

public class ChangeTheme : CommandDefinition
{
   const string NAME = nameof(ChangeTheme);
   const string ACTIVATION_CONTEXT = "The user wants to change the application theme";
   const string RETURN_DESCRIPTION = "What has been changed";
   const string EXAMPLES = """
      UserRequest: I don't like the color of the background, I'd like it to be blue
      JSON: { "WhatToChange": "Change the background color to blue" }
      
      UserRequest: I'd like a colorful theme for the application
      JSON: { "WhatToChange": "Set the theme color to a colorful one" }
      """;

   private readonly List<CommandArgumentDefinition> _arguments = new()
   {
      new CommandArgumentDefinition("WhatToChange", "What the user wants to change in general about the theme.",false),
   };
   readonly ILogger<ChangeTheme> _logger;
   readonly IEnumerable<ICompletionConnector> _conversationalConnectors;

   public ChangeTheme(ILogger<ChangeTheme> logger, IEnumerable<ICompletionConnector> conversationalConnectors)
      : base(NAME, ACTIVATION_CONTEXT, RETURN_DESCRIPTION, EXAMPLES)
   {
      _logger = logger;
      _conversationalConnectors = conversationalConnectors;

      Arguments = _arguments;
   }

   public override async ValueTask<string?> ExecuteAsync(string userPrompt, string? inputPrompt)
   {
      if (string.IsNullOrWhiteSpace(inputPrompt))
      {
         _logger.LogWarning("The input prompt is empty");
         return null;
      }

      var args = JsonSerializer.Deserialize<Args>(inputPrompt);

      if (args == null)
      {
         _logger.LogWarning("The input prompt is not valid JSON: {inputPrompt}", inputPrompt);
         return null;
      }


      var sb = new StringBuilder("""
         This is the definition of a light palette
         ```c#
         new PaletteLight
         {
            Primary = FromRgb(255, 152, 0), // Light orange
            Secondary = FromRgb(0, 121, 107), // Dark teal
            Tertiary = FromRgb(255, 110, 0), // Bright orange
            Background = FromRgb(249, 249, 249), // Light gray
            Surface = FromRgb(255, 255, 255), // White
            DrawerBackground = FromRgb(242, 242, 242), // Lighter gray
            DrawerText = FromRgb(51, 51, 51), // Dark gray
            AppbarBackground = FromRgb(255, 255, 255), // White
            AppbarText = FromRgb(51, 51, 51), // Dark gray
            TextPrimary = FromRgb(51, 51, 51), // Dark gray
            TextSecondary = FromRgb(153, 153, 153), // Light gray
            TextDisabled = FromRgb(204, 204, 204), // Lighter gray
            ActionDefault = FromRgb(51, 51, 51), // Dark gray
            ActionDisabled = FromRgb(204, 204, 204), // Lighter gray
            Divider = FromRgb(224, 224, 224), // Light gray
            DividerLight = FromRgb(240, 240, 240), // Lighter gray
            TableLines = FromRgb(240, 240, 240), // Lighter gray
            Info = FromRgb(0, 153, 204), // Bright blue
            Success = FromRgb(0, 204, 102), // Bright green
            Warning = FromRgb(255, 193, 7), // Yellow
            Error = FromRgb(220, 53, 69) // Red
         }
         ```
            
         """);

      if (args.WhatToChange is not null)
      {
         sb.AppendLine($"Modify the palette based on this user request: {args.WhatToChange}");
      }
      else
      {
         sb.AppendLine($"Modify the palette based on this user request: {userPrompt}");
      }

      sb.AppendLine("```c#");

      var connector = _conversationalConnectors.FirstOrDefault(c => c.Enabled)
         ?? throw new InvalidOperationException("No conversational connector is enabled");

      var response = await connector.RequestCompletionAsync(new CompletionRequest
      {
         Prompt = sb.ToString(),
         MaxGeneratedTokens = 500
      }).ConfigureAwait(false);

      return response?.GeneratedMessage;
   }

   record Args
   {
      public string? WhatToChange { get; set; }
   }
}

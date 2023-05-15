using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AIdentities.Shared.Features.Core.Services;

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
   readonly IThemeManager _themeManager;

   public ChangeTheme(ILogger<ChangeTheme> logger, IEnumerable<ICompletionConnector> conversationalConnectors, IThemeManager themeManager)
      : base(NAME, ACTIVATION_CONTEXT, RETURN_DESCRIPTION, EXAMPLES)
   {
      _logger = logger;
      _conversationalConnectors = conversationalConnectors;
      _themeManager = themeManager;

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
         
         I want you to write a json representation of this palette, where the color
         is specified as a hex value (e.g. #FF9800).

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

      var theme = _themeManager.GetTheme()!;

      bool success = false;
      int retryAttempt = 0;
      do
      {
         var response = await connector.RequestCompletionAsync(new DefaultCompletionRequest
         {
            Prompt = sb.ToString(),
            MaxGeneratedTokens = 500
         }).ConfigureAwait(false);

         try
         {
            var json = Regex.Match(response?.GeneratedMessage!, @"\{(.|\s)*\}", RegexOptions.Multiline).Value;
            var paletteDto = JsonSerializer.Deserialize<Palette>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (paletteDto == null)
            {
               _logger.LogWarning("The generated message is not valid JSON: {generatedMessage}", response?.GeneratedMessage);
               return null;
            }

            var palette = CreatePalette(paletteDto);

            theme.Palette = palette;
            theme.PaletteDark = palette;

            _themeManager.SetTheme(theme);

            return "The palette has been changed";
         }
         catch (Exception ex)
         {
            _logger.LogError(ex, "Error while deserializing the generated message: {generatedMessage}", response?.GeneratedMessage);

            if (retryAttempt++ >= 3)
            {
               return response?.GeneratedMessage;
            }
         }
      } while (!success && retryAttempt < 3);

      return "Some error occurred while changing the palette";
   }

   private PaletteLight CreatePalette(Palette paletteDto)
   {
      return new PaletteLight()
      {
         Primary = paletteDto.Primary,
         Secondary = paletteDto.Secondary,
         Tertiary = paletteDto.Tertiary,
         Background = paletteDto.Background,
         Surface = paletteDto.Surface,
         DrawerBackground = paletteDto.DrawerBackground,
         DrawerText = paletteDto.DrawerText,
         AppbarBackground = paletteDto.AppbarBackground,
         AppbarText = paletteDto.AppbarText,
         TextPrimary = paletteDto.TextPrimary,
         TextSecondary = paletteDto.TextSecondary,
         TableLines = paletteDto.TableLines,
         ActionDefault = paletteDto.ActionDefault,
         ActionDisabled = paletteDto.ActionDisabled,
         Divider = paletteDto.Divider,
         DividerLight = paletteDto.DividerLight,
         Error = paletteDto.Error,
         Info = paletteDto.Info,
         Success = paletteDto.Success,
         TextDisabled = paletteDto.TextDisabled,
         Warning = paletteDto.Warning
      };
   }

   record Args
   {
      public string? WhatToChange { get; set; }
   }

   public record Palette
   {
      public string? Primary { get; set; }
      public string? Secondary { get; set; }
      public string? Tertiary { get; set; }
      public string? Background { get; set; }
      public string? Surface { get; set; }
      public string? DrawerBackground { get; set; }
      public string? DrawerText { get; set; }
      public string? AppbarBackground { get; set; }
      public string? AppbarText { get; set; }
      public string? TextPrimary { get; set; }
      public string? TextSecondary { get; set; }
      public string? TextDisabled { get; set; }
      public string? ActionDefault { get; set; }
      public string? ActionDisabled { get; set; }
      public string? Divider { get; set; }
      public string? DividerLight { get; set; }
      public string? TableLines { get; set; }
      public string? Info { get; set; }
      public string? Success { get; set; }
      public string? Warning { get; set; }
      public string? Error { get; set; }
      public string? Theme { get; set; }
   }
}

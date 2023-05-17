using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace AIdentities.BrainButler.Commands.ChangeTheme;

public partial class ChangeThemeCommand : CommandDefinition
{
   public const string NAME = nameof(ChangeThemeCommand);
   const string ACTIVATION_CONTEXT = "The user wants to change the application theme";
   const string RETURN_DESCRIPTION = "What has been changed";
   const string ARGUMENT_WHAT_TO_CHANGE = "WhatToChange";
   const string ARGUMENT_IS_DARK_PALETTE = "IsDarkPalette";
   const string EXAMPLES = $$"""
      UserRequest: I don't like the color of the background, I want a dark theme and I want it to be blue
      JSON: { "{{ARGUMENT_WHAT_TO_CHANGE}}": "Change the background of the dark palette to blue", "{{ARGUMENT_IS_DARK_PALETTE}}": true }
      
      UserRequest: I'd like a colorful theme for the application
      JSON: { "{{ARGUMENT_WHAT_TO_CHANGE}}": "Set the theme color to a colorful one", "{{ARGUMENT_IS_DARK_PALETTE}}": false }
      """;

   static readonly JsonSerializerOptions _jsonOptionExample = new() { WriteIndented = true };

   private readonly List<CommandArgumentDefinition> _arguments = new()
   {
      new CommandArgumentDefinition(ARGUMENT_WHAT_TO_CHANGE, "What the user wants to change in general about the theme.", true),
      new CommandArgumentDefinition(ARGUMENT_IS_DARK_PALETTE, "Nullable boolean value that specifies if the user want to create a dark palette or not. If you aren't sure, don't set the argument.", false),
   };
   readonly ILogger<ChangeThemeCommand> _logger;
   readonly IEnumerable<ICompletionConnector> _conversationalConnectors;
   readonly ThemeUpdater _themeUpdater;
   readonly IPluginStorage<PluginEntry> _pluginStorage;

   public ChangeThemeCommand(ILogger<ChangeThemeCommand> logger,
                             IEnumerable<ICompletionConnector> conversationalConnectors,
                             IPluginStorage<PluginEntry> pluginStorage,
                             ThemeUpdater themeUpdater
                             )
      : base(NAME, ACTIVATION_CONTEXT, RETURN_DESCRIPTION, EXAMPLES)
   {
      _logger = logger;
      _conversationalConnectors = conversationalConnectors;
      _pluginStorage = pluginStorage;
      _themeUpdater = themeUpdater;

      Arguments = _arguments;
   }

   public override async IAsyncEnumerable<CommandExecutionStreamedFragment> ExecuteAsync(string userPrompt,
                                                                string? inputPrompt,
                                                                [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      var connector = _conversationalConnectors.FirstOrDefault(c => c.Enabled)
         ?? throw new InvalidOperationException("No conversational connector is enabled");

      if (string.IsNullOrWhiteSpace(inputPrompt))
      {
         yield return new CommandExecutionStreamedFragment("The input prompt is empty");
         yield break;
      }

      var args = JsonSerializer.Deserialize<Args>(inputPrompt);
      if (args == null)
      {
         yield return new CommandExecutionStreamedFragment($"I couldn't properly execute the command because I haven't generated a valid JSON out of my thoughts: {inputPrompt}");
         yield break;
      }

      string prompt = GeneratePromptToHaveNewJsonPalette(userPrompt, args);

      bool success = false;
      int retryAttempt = 3;
      do
      {
         yield return new CommandExecutionStreamedFragment($"Let me think about the new palette...");

         string response = string.Empty;

         var outputBuilder = new StringBuilder();
         var completions = connector.RequestCompletionAsStreamAsync(new DefaultCompletionRequest
         {
            Prompt = prompt,
            MaxGeneratedTokens = 500
         }, cancellationToken)
            .WithCancellation(cancellationToken)
            .ConfigureAwait(false);
         await foreach (var completion in completions)
         {
            outputBuilder.Append(completion.GeneratedMessage);
         }


         response = outputBuilder.ToString();
         if (response == string.Empty)
         {
            _logger.LogWarning("The generated message is empty");
            continue;
         }

         success = ApplyThemeChanges(response, args.IsDarkPalette, out var proposedPalette);
         retryAttempt--;

         if (!success)
         {
            yield return new CommandExecutionStreamedFragment($"mmm... I think I've failed to change the palette. Let me try again...");
         }
         else
         {
            yield return new CommandExecutionStreamedFragment($"For you I thought to give you this palette");
            await _themeUpdater.SavePaletteToDisk(proposedPalette, args.IsDarkPalette).ConfigureAwait(false);
         }
      } while (!success && retryAttempt < 3);

      if (!success)
      {
         yield return new CommandExecutionStreamedFragment($"I've tried to change the palette {retryAttempt} times, but I failed, shame on me :(");
         yield break;
      }

      yield return new CommandExecutionStreamedFragment($"I've changed the palette, I hope you like it");
   }

   private string GeneratePromptToHaveNewJsonPalette(string userPrompt, Args args)
   {
      var paletteType = args.IsDarkPalette ?? _themeUpdater.IsDarkMode ? "dark" : "light";

      var sb = new StringBuilder($"""
         This is the definition of a {paletteType} palette
         ```json
         {BuildCurrentJsonPalette(args.IsDarkPalette)}
         ```
         
         I want you to write a json representation of this palette, where the color is specified as a hex value (e.g. #FF9800).
         When you change the palette, adapt all the colors to the new palette and follow this rules for an effective dark mode:
         
         Understand Color Theory: Know the basics of hue, saturation, and brightness. This knowledge will guide your color choices.
         Ensure Contrast: Maintain high contrast between your background and foreground elements to ensure legibility. Use tools like the Web Content Accessibility Guidelines (WCAG) to check your contrast levels.
         Select Your Color Scheme: Monochromatic and analogous color schemes often work well for dark mode. Choose a scheme that complements your design.
         Manage Color Temperature: Remember, warmer colors appear to advance and cooler colors seem to recede in dark mode. Use this to guide users' attention in your interface.
         Desaturate Colors: Colors appear more saturated against a dark background. Counteract this by desaturating your colors in dark mode.
         Use Semantic Colors: Colors can convey meaning, so be mindful of this when designing. Make sure your color choices don't confuse or misinterpret.
         Sparingly Use Brand Colors: Use your brand colors sparingly and ensure they have enough contrast when used in dark mode.
         Apply Transparency: Use transparency to create depth and hierarchy in your UI. But be careful to maintain legibility.
         Test Your Design: Test your dark mode design under various conditions to ensure it's effective for all users. Make adjustments based on feedback and observations.
         Remember, creating an effective dark theme requires a balance of aesthetics and functionality. Always keep your users' needs in mind.

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

      return sb.ToString();
   }

   private bool ApplyThemeChanges(string response, bool? isDarkPalette, out string proposedPalette)
   {
      proposedPalette = ExtractJson().Match(response).Value;
      _logger.LogDebug("The generated palette is: {generatedPalette}", proposedPalette);

      try
      {
         var paletteDto = JsonSerializer.Deserialize<PaletteReference>(proposedPalette, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
         if (paletteDto == null)
         {
            _logger.LogWarning("The generated message is not valid JSON: {generatedMessage}", response);
            return false;
         }

         var theme = _themeUpdater.GetTheme()!;
         if (isDarkPalette ?? _themeUpdater.IsDarkMode)
         {
            theme.PaletteDark = PaletteMapper.CreateDarkPalette(paletteDto);
         }
         else
         {
            theme.Palette = PaletteMapper.CreateLightPalette(paletteDto);
         }
         _themeUpdater.SetTheme(theme);
         return true;
      }
      catch (Exception ex)
      {
         _logger.LogError(ex, "Error while deserializing the generated message: {generatedMessage}", response);
         return false;
      }
   }

   /// <summary>
   /// Based on the current theme, build a json representation of the palette to
   /// insert it in the generated prompt.
   /// </summary>
   /// <returns>The json representation of the current palette</returns>
   public string BuildCurrentJsonPalette(bool? isDarkMode)
   {
      MudTheme theme = _themeUpdater.GetTheme()!;
      var palette = isDarkMode ?? _themeUpdater.IsDarkMode ? theme.PaletteDark : theme.Palette;

      var paletteReference = new PaletteReference()
      {
         Primary = palette.Primary.ToString(),
         Secondary = palette.Secondary.ToString(),
         Tertiary = palette.Tertiary.ToString(),
         Background = palette.Background.ToString(),
         Surface = palette.Surface.ToString(),
         DrawerBackground = palette.DrawerBackground.ToString(),
         DrawerText = palette.DrawerText.ToString(),
         AppbarBackground = palette.AppbarBackground.ToString(),
         AppbarText = palette.AppbarText.ToString(),
         TextPrimary = palette.TextPrimary.ToString(),
         TextSecondary = palette.TextSecondary.ToString(),
         TableLines = palette.TableLines.ToString(),
         ActionDefault = palette.ActionDefault.ToString(),
         ActionDisabled = palette.ActionDisabled.ToString(),
         Divider = palette.Divider.ToString(),
         DividerLight = palette.DividerLight.ToString(),
         TextDisabled = palette.TextDisabled.ToString(),
      };

      return JsonSerializer.Serialize(paletteReference, _jsonOptionExample);
   }

   [GeneratedRegex("\\{(.|\\s)*\\}", RegexOptions.Multiline)]
   private static partial Regex ExtractJson();
}

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace AIdentities.BrainButler.Commands.ChangeTheme;

public class ChangeTheme : CommandDefinition
{
   public const string NAME = nameof(ChangeTheme);
   const string ACTIVATION_CONTEXT = "The user wants to change the application theme";
   const string RETURN_DESCRIPTION = "What has been changed";
   const string ARGUMENT_WHAT_TO_CHANGE = "WhatToChange";
   const string ARGUMENT_IS_DARK_PALETTE = "IsDarkPalette";
   const string EXAMPLES = $$"""
      UserRequest: I don't like the color of the background, I want a dark theme and I want it to be blue
      Reasoning: The user is asking to change the background of the dark palette to blue, {{ARGUMENT_IS_DARK_PALETTE}} is dark.
      JSON: { "{{ARGUMENT_WHAT_TO_CHANGE}}": "Change the background of the dark palette to blue", "{{ARGUMENT_IS_DARK_PALETTE}}": true }
      
      UserRequest: I'd like a colorful theme for the application
      Reasoning: The user is asking to create a colorful palette, usually dark palettes aren't dark but I can't be sure: {{ARGUMENT_IS_DARK_PALETTE}} is null.
      JSON: { "{{ARGUMENT_WHAT_TO_CHANGE}}": "Set the theme color to a colorful one", "{{ARGUMENT_IS_DARK_PALETTE}}": false }
      """;

   const string DARK_PALETTE_INSTRUCTION = """
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
      """;

   const string LIGHT_PALETTE_INSTRUCTION = """
      Understand Color Theory: Just as with dark themes, understanding the basics of color theory is crucial for light themes. Understanding how different colors interact with each other can guide your color choices.
      Ensure Readability: Light themes can sometimes strain the eyes, especially when there's not enough contrast between text and background. Make sure that your text is easily readable against your background colors.
      Choose Your Color Scheme Wisely: In light themes, you have a wider color palette to work with. However, be sure to choose colors that complement each other and the overall design. 
      Manage Color Temperature: In light mode, cooler colors can provide a calming effect, while warmer colors may add vibrancy and energy. 
      Avoid Over-Saturation: Overly bright and saturated colors can cause visual fatigue. Opt for softer, less saturated colors to avoid overwhelming the user.
      Use Semantic Colors: Color can convey meaning, so be sure to use colors that are consistent with the rest of your design and the meanings they're meant to convey.
      Balanced Use of Brand Colors: While it's important to incorporate your brand colors, don't let them dominate the interface. Balance them with neutral tones to create a harmonious design.
      Use Shadows for Depth: Unlike dark themes, where transparency can be used for depth, light themes often benefit from the use of shadows. This can help to create hierarchy and depth in your design.
      Consider Dark Text: For light themes, dark text tends to be more readable than light text. However, the specific color should depend on the background color to maintain good contrast.
      Test Your Design: As always, testing is vital. Make sure to test your light theme under various conditions, with different users, and adjust based on feedback.
      Just like with dark themes, the key to creating an effective light theme is to balance aesthetics and functionality, keeping the needs of your users in mind.
      
      """;

   static readonly JsonSerializerOptions _jsonOptionExample = new() { WriteIndented = true };

   private readonly List<CommandArgumentDefinition> _arguments = new()
   {
      new CommandArgumentDefinition(ARGUMENT_WHAT_TO_CHANGE, "What the user wants to change in general about the theme.", true),
      new CommandArgumentDefinition(ARGUMENT_IS_DARK_PALETTE, "Nullable boolean value that specifies if the user want to create a dark palette or not. If you aren't sure, don't set the argument.", false),
   };
   readonly ILogger<ChangeTheme> _logger;
   readonly IConnectorsManager _connectorsManager;
   readonly ThemeUpdater _themeUpdater;
   readonly IPluginStorage<PluginEntry> _pluginStorage;

   public ChangeTheme(ILogger<ChangeTheme> logger,
                             IConnectorsManager connectorsManager,
                             IPluginStorage<PluginEntry> pluginStorage,
                             ThemeUpdater themeUpdater
                             )
      : base(NAME, ACTIVATION_CONTEXT, RETURN_DESCRIPTION, EXAMPLES)
   {
      _logger = logger;
      _connectorsManager = connectorsManager;
      _pluginStorage = pluginStorage;
      _themeUpdater = themeUpdater;

      Arguments = _arguments;
   }

   public override async IAsyncEnumerable<CommandExecutionStreamedFragment> ExecuteAsync(string userPrompt,
                                                                string? inputPrompt,
                                                                [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      var connector = _connectorsManager.GetCompletionConnector()
         ?? throw new InvalidOperationException("No completion connector is enabled");

      if (string.IsNullOrWhiteSpace(inputPrompt))
      {
         yield return new CommandExecutionStreamedFragment("The input prompt is empty");
         yield break;
      }

      if (!TryExtractJson<Args>(inputPrompt, out var args))
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
         int consumedPaletteTokens = 0;
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
            consumedPaletteTokens = completion.CumulativeCompletionTokens ?? 0;
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
            yield return new CommandExecutionStreamedFragment($"For you I thought to give you this palette. The palette has been generated by using {consumedPaletteTokens} tokens.");
            await _themeUpdater.SavePaletteToDisk(proposedPalette!, args.IsDarkPalette).ConfigureAwait(false);
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
      bool isDarkPalette = args.IsDarkPalette ?? _themeUpdater.IsDarkMode;
      var paletteType = isDarkPalette ? "dark" : "light";

      var theme_instruction = isDarkPalette
         ? DARK_PALETTE_INSTRUCTION
         : LIGHT_PALETTE_INSTRUCTION;

      var sb = new StringBuilder($"""
         This is the definition of a {paletteType} palette
         ```json
         {BuildCurrentJsonPalette(args.IsDarkPalette)}
         ```
         
         I want you to write a json representation of this palette, where the color is specified as a hex value (e.g. #FF9800).
         When you change the palette, adapt all the colors to the new palette and follow this rules for an effective theme:
         
         {theme_instruction}

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

   private bool ApplyThemeChanges(string response, bool? isDarkPalette, [MaybeNullWhen(false)] out string proposedPalette)
   {

      if (!TryExtractJson(response, out proposedPalette))
      {
         return false;
      }

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
}

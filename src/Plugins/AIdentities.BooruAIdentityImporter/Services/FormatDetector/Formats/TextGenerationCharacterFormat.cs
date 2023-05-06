namespace AIdentities.BooruAIdentityImporter.Services.FormatDetector.Formats;

public class TextGenerationCharacterFormat : BaseCharacterFormat
{
   public override string Format => "TextGenerationCharacter";
   protected override string[] Properties => new[] { "char_name", "char_persona", "world_scenario", "char_greeting", "example_dialogue" };
}

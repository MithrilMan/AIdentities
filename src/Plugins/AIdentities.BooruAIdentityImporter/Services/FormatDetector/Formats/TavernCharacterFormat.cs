namespace AIdentities.BooruAIdentityImporter.Services.FormatDetector.Formats;

public class TavernCharacterFormat : BaseCharacterFormat
{
   public override string Format => "TavernCharacter";
   protected override string[] Properties => new[] { "name", "description", "personality", "scenario", "first_mes", "mes_example" };
}

namespace AIdentities.BooruAIdentityImporter.Services.FormatDetector.Formats;

public class CaiCharacterFormat : BaseCharacterFormat
{
   public override string Format => "CaiCharacter";
   protected override string[] Properties => new[] { "name", "title", "description", "greeting", "definition" };
}

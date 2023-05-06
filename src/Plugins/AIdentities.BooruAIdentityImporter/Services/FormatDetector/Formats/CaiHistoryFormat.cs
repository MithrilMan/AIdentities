namespace AIdentities.BooruAIdentityImporter.Services.FormatDetector.Formats;

public class CaiHistoryFormat : BaseCharacterFormat
{
   public override string  Format => "CaiHistory";
   protected override string[] Properties => new[] { "name", "title", "description", "greeting" };
}

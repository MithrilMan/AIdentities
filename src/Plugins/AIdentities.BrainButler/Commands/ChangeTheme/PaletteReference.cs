namespace AIdentities.BrainButler.Commands.ChangeTheme;

public record PaletteReference
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
}

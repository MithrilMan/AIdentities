namespace AIdentities.UI.Components.Mentions;

public interface IMention
{
   char Marker { get; set; }
   string Text { get; set; }
   string Value { get; set; }
   string Description { get; set; }
   string? Avatar { get; set; }
}

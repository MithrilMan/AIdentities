using Toolbelt.Blazor.HotKeys2;

namespace AIdentities.Shared.Features.Core.Components;
public partial class HotKeyCheatSheet
{
   [Parameter, EditorRequired]
   public HotKeysContext? HotKeysContext { get; set; }


   public IEnumerable<HotKeyEntry> GetHotKeys()
      => HotKeysContext?.Keys.Where(k => !string.IsNullOrEmpty(k.Description))
      ?? Enumerable.Empty<HotKeyEntry>();
}

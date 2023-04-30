namespace AIdentities.Shared.Plugins.Pages;
public sealed class EmptyAppPageSettings : IAppComponentSettings
{
   public static EmptyAppPageSettings Instance { get; } = new EmptyAppPageSettings();
}

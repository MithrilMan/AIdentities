namespace AIdentities.Shared.Plugins.Pages;
public interface IAppComponent
{
   CancellationToken CancellationToken { get; }
   Task SignalComponentStateHasChanged();
}

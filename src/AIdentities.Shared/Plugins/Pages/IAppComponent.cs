namespace AIdentities.Shared.Plugins.Pages;
public interface IAppComponent
{
   CancellationToken PageCancellationToken { get; }
   Task SignalComponentStateHasChanged();
}

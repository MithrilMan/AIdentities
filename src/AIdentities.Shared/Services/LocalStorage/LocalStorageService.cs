namespace AIdentities.Shared.Services.LocalStorage;
internal class LocalStorageService : ILocalStorage
{
   readonly ILogger<LocalStorageService> _logger;
   readonly Blazored.LocalStorage.ILocalStorageService _localStorageService;

   public event EventHandler<StorageItemChangingEventArgs>? Changing;
   public event EventHandler<StorageItemChangedEventArgs>? Changed;

   public LocalStorageService(ILogger<LocalStorageService> logger, Blazored.LocalStorage.ILocalStorageService localStorageService)
   {
      _logger = logger;
      _localStorageService = localStorageService;
      _localStorageService.Changing += LocalStorageService_Changing;
      _localStorageService.Changed += LocalStorageService_Changed;
   }

   private void LocalStorageService_Changed(object? sender, Blazored.LocalStorage.ChangedEventArgs e) => Changed?.Invoke(this, new(e.Key, e.OldValue, e.NewValue));

   private void LocalStorageService_Changing(object? sender, Blazored.LocalStorage.ChangingEventArgs e) => Changing?.Invoke(this, new(e.Key, e.OldValue, e.NewValue));

   public ValueTask ClearAsync(CancellationToken cancellationToken = default) => _localStorageService.ClearAsync(cancellationToken);

   public ValueTask<bool> ContainKeyAsync(string key, CancellationToken cancellationToken = default) => _localStorageService.ContainKeyAsync(key, cancellationToken);

   public ValueTask<string> GetItemAsStringAsync(string key, CancellationToken cancellationToken = default) => _localStorageService.GetItemAsStringAsync(key, cancellationToken);
   public ValueTask<T> GetItemAsync<T>(string key, CancellationToken cancellationToken = default) => _localStorageService.GetItemAsync<T>(key, cancellationToken);

   public ValueTask RemoveItemAsync(string key, CancellationToken cancellationToken = default) => _localStorageService.RemoveItemAsync(key, cancellationToken);
   public ValueTask RemoveItemsAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default) => _localStorageService.RemoveItemsAsync(keys, cancellationToken);

   public ValueTask SetItemAsStringAsync(string key, string data, CancellationToken cancellationToken = default) => _localStorageService.SetItemAsStringAsync(key, data, cancellationToken);
   public ValueTask SetItemAsync<T>(string key, T data, CancellationToken cancellationToken = default) => _localStorageService.SetItemAsync(key, data, cancellationToken);
}

namespace AIdentities.Shared.Services.LocalStorage;
interface ILocalStorage
{
   /// <summary>
   /// Occurs when an item is about to be changed.
   /// </summary>
   event EventHandler<StorageItemChangingEventArgs> Changing;

   /// <summary>
   /// Occurs when an item has been changed.
   /// </summary>
   event EventHandler<StorageItemChangedEventArgs> Changed;


   ValueTask<bool> ContainKeyAsync(string key, CancellationToken cancellationToken = default);

   ValueTask ClearAsync(CancellationToken cancellationToken = default);

   ValueTask<T> GetItemAsync<T>(string key, CancellationToken cancellationToken = default);

   ValueTask<string> GetItemAsStringAsync(string key, CancellationToken cancellationToken = default);

   ValueTask RemoveItemAsync(string key, CancellationToken cancellationToken = default);

   ValueTask RemoveItemsAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);

   ValueTask SetItemAsync<T>(string key, T data, CancellationToken cancellationToken = default);

   ValueTask SetItemAsStringAsync(string key, string data, CancellationToken cancellationToken = default);
}

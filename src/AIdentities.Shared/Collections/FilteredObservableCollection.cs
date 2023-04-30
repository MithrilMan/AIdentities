using System.Collections.ObjectModel;

namespace AIdentities.Shared.Collections;

public sealed class FilteredObservableCollection<TItem>
{
   private readonly List<TItem> _unfilteredItems;
   private readonly RangeObservableCollection<TItem> _filteredItems;
   readonly Func<IEnumerable<TItem>, ValueTask<IEnumerable<TItem>>> _filterMethod;

   public IReadOnlyCollection<TItem> UnfilteredItems => _unfilteredItems;

   public ObservableCollection<TItem> Items => _filteredItems;

   public int FilteredCount => _filteredItems.Count;
   public int UnfilteredCount => _unfilteredItems.Count;

   public FilteredObservableCollection(Func<IEnumerable<TItem>, ValueTask<IEnumerable<TItem>>> filterMethod)
   {
      _unfilteredItems = new List<TItem>(0);
      _filteredItems = new RangeObservableCollection<TItem>();

      _filterMethod = filterMethod ?? throw new ArgumentNullException(nameof(filterMethod));
   }

   /// <summary>
   /// Applica il filtro configurato ed aggiorna l'elenco degli item filtrati.
   /// </summary>
   public async ValueTask ApplyFilterAsync()
   {
      var filteredItems = await _filterMethod(_unfilteredItems).ConfigureAwait(false);
      _filteredItems.ReplaceRange(filteredItems);
   }

   /// <summary>
   /// Sostituisce un elemento dalla lista di elementi non filtrati (raw) ed applica il filtro.
   /// </summary>
   /// <param name="itemToReplace">Elemento da rimuovere.</param>
   /// <param name="newInstance">Elemento che sostituisce l'elemento da rimuovere.</param>
   /// <returns><see langword="true"/> se l'item è stato sostituito, altrimenti <see langword="false"/>.</returns>
   public async ValueTask<bool> ReplaceItemAsync(TItem itemToReplace, TItem newInstance)
   {
      var index = _unfilteredItems.IndexOf(itemToReplace);
      if (index < 0) return false;

      _unfilteredItems[index] = newInstance;

      await ApplyFilterAsync().ConfigureAwait(false);
      return true;
   }

   /// <summary>
   /// Restituisce l'indice dell'item corrispondente all'insieme di elementi non filtrati (raw).
   /// </summary>
   /// <param name="item">Item da ricercare all'interno degli item non filtrati.</param>
   /// <returns>Indice della prima occorrenza dell'item specificato, o -1 se l'item non è stato trovato</returns>
   public int IndexOf(TItem item) => _unfilteredItems.IndexOf(item);

   /// <summary>
   /// Inserisce un item tra gli items non filtrati (raw) e applica nuovamente il sorting agli items filtrati.
   /// </summary>
   /// <param name="index"></param>
   /// <param name="itemToInsert"></param>
   /// <returns></returns>
   public async ValueTask<bool> InsertAsync(int index, TItem itemToInsert)
   {
      if (index < 0 || index >= _unfilteredItems.Count) return false;

      _unfilteredItems.Insert(index, itemToInsert);

      await ApplyFilterAsync().ConfigureAwait(false);
      return true;
   }

   /// <summary>
   /// Popola/sostituisce la lista di elementi non filtrati (raw) ed applica il filtro.
   /// </summary>
   /// <param name="items"></param>
   public ValueTask LoadItemsAsync(IEnumerable<TItem>? items)
   {
      _unfilteredItems.Clear();
      if (items is not null)
         _unfilteredItems.AddRange(items);

      return ApplyFilterAsync();
   }

   /// <summary>
   /// Rimuove un item dall'elenco degli items non filtrati (raw) ed applica il filtro.
   /// </summary>
   /// <param name="itemToRemove">Elemento da rimuovere.</param>
   /// <returns><see langword="true"/> se l'item è stato rimosso, altrimenti <see langword="false"/>.</returns>
   public async ValueTask<bool> RemoveItemAsync(TItem itemToRemove)
   {
      var removed = _unfilteredItems.Remove(itemToRemove);
      if (removed)
         await ApplyFilterAsync().ConfigureAwait(false);

      return removed;
   }

   public async ValueTask AppendItemAsync(TItem item)
   {
      _unfilteredItems.Add(item);
      await ApplyFilterAsync().ConfigureAwait(false);
   }
}

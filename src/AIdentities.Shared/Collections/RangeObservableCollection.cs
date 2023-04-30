using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace AIdentities.Shared.Collections;

public sealed class RangeObservableCollection<T> : ObservableCollection<T>
{
   public RangeObservableCollection() : base() { }

   public RangeObservableCollection(IEnumerable<T> collection) : base(collection) { }

   public void AddRange(IEnumerable<T> collection)
   {
      if (collection is null) throw new ArgumentNullException(nameof(collection));

      foreach (var i in collection)
      {
         Items.Add(i);
      }

      OnCountPropertyChanged();
      OnIndexerPropertyChanged();
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems: collection.ToList(), oldItems: new List<T>()));
   }

   /// <summary>
   /// Bisogna assicurarsi che la collection che viene passata non sia un'enumerable della collection che si vuole modificare.
   /// </summary>
   /// <param name="collection"></param>
   /// <exception cref="ArgumentNullException"></exception>
   public void RemoveRange(IEnumerable<T> collection)
   {
      if (collection is null) throw new ArgumentNullException(nameof(collection));

      foreach (var i in collection)
      {
         Items.Remove(i);
      }

      OnCountPropertyChanged();
      OnIndexerPropertyChanged();
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems: new List<T>(), oldItems: collection.ToList()));
   }

   public void ReplaceRange(IEnumerable<T> collection)
   {
      if (collection is null) throw new ArgumentNullException(nameof(collection));

      var old = new List<T>();
      old.AddRange(Items);

      Items.Clear();
      foreach (var i in collection)
      {
         Items.Add(i);
      }

      OnCountPropertyChanged();
      OnIndexerPropertyChanged();
      OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems: collection.ToList(), oldItems: old));
   }

   public void ReplaceItem(Predicate<T> predicate, T newInstance)
   {
      var item = Items.FirstOrDefault(f => predicate(f));
      if (item == null) return;

      var index = Items.IndexOf(item);
      if (index < 0) return;

      SetItem(index, newInstance);
   }

   /// <summary>
   /// Helper to raise a PropertyChanged event for the Count property
   /// </summary>
   private void OnCountPropertyChanged() => OnPropertyChanged(EventArgsCache.CountPropertyChanged);

   /// <summary>
   /// Helper to raise a PropertyChanged event for the Indexer property
   /// </summary>
   private void OnIndexerPropertyChanged() => OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);

   internal static class EventArgsCache
   {
      internal static readonly PropertyChangedEventArgs CountPropertyChanged = new PropertyChangedEventArgs("Count");
      internal static readonly PropertyChangedEventArgs IndexerPropertyChanged = new PropertyChangedEventArgs("Item[]");
   }
}

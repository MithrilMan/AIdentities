namespace AIdentities.Shared.Services.LocalStorage;

/// <summary>
/// Raised when a value in the local storage is changed.
/// </summary>
/// <param name="Key">The key of the item that was changed.</param>
/// <param name="OldValue">The old value of the item.</param>
/// <param name="NewValue">The new value of the item.</param>
public record StorageItemChangedEventArgs(string Key, object OldValue, object NewValue);

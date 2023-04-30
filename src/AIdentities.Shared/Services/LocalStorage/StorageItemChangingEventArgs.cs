namespace AIdentities.Shared.Services.LocalStorage;

/// <summary>
/// Raised when a value in the local storage is changing.
/// </summary>
/// <param name="Key">The key of the item that was changed.</param>
/// <param name="OldValue">The old value of the item.</param>
/// <param name="NewValue">The new value of the item.</param>
public record StorageItemChangingEventArgs(string Key, object OldValue, object NewValue)
{
   /// <summary>
   /// Gets or sets a value indicating whether the change should be canceled.
   /// </summary>
   public bool Cancel { get; set; } = false;
};

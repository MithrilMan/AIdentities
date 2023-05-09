namespace AIdentities.Shared.Features.Core.Abstracts;

public interface IPluginSettingsTab
{
   /// <summary>
   /// Requests the component to save any changes made to the settings and return an updated version of it.
   /// </summary>
   /// <returns>
   /// The updated settings or null if the saving failed.
   /// There is no need to inform the user about the failure, it will be handled by the component owning this tab.
   /// </returns>
   Task<object?> SaveAsync();

   /// <summary>
   /// Requests the component to undo any changes made to the settings.
   /// </summary>
   Task UndoChangesAsync();
}

public interface IPluginSettingsTab<TPluginSettings> : IPluginSettingsTab
   where TPluginSettings : IPluginSettings
{
   /// <summary>
   /// Requests the component to save any changes made to the settings and return an updated version of it.
   /// </summary>
   /// <returns>
   /// The updated settings or null if the saving failed.
   /// There is no need to inform the user about the failure, it will be handled by the component owning this tab.
   /// </returns>
   new Task<TPluginSettings?> SaveAsync();

   /// <summary>
   /// Requests the component to undo any changes made to the settings.
   /// </summary>
   new Task UndoChangesAsync();
}

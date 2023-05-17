using AIdentities.Shared.Features.Core.Abstracts;
using Microsoft.AspNetCore.Components;

namespace AIdentities.Shared.Features.Core.Components;

/// <summary>
/// A base class useful for all the plugin settings tab.
/// A plugin settings tab isn't forced to inherit from this class, but it's useful to have because
/// implements already useful code that it's required most of the time.
/// </summary>
/// <remarks>
/// Usually the developer has to creata a custom class that inherits from this class and implement the 
/// abstract methos <see cref="AreSettingsValid"/>, <see cref="SaveAsync"/> and <see cref="CreateState"/>.
/// </remarks>
/// <typeparam name="TPluginSettings">The type of the plugin settings.</typeparam>
public abstract class BasePluginSettingsTab<TPluginSettings, TState> : ComponentBase, IPluginSettingsTab<TPluginSettings>
   where TPluginSettings : class, IPluginSettings
   where TState : BasePluginSettingsTab<TPluginSettings, TState>.BaseState, new()
{
   [Parameter] public bool IsChanged { get; set; } = default!;
   [Parameter] public TPluginSettings PluginSettings { get; set; } = default!;

   /// <summary>
   /// Returns a plugin settings containing the updated values.
   /// Note that this method will be called only if the <see cref="AreSettingsValid"/> method returns true.
   /// </summary>
   /// <returns></returns>
   public abstract Task<TPluginSettings> PerformSavingAsync();
   /// <summary>
   /// Ensure that the settings filled by the user are valid.
   /// It's a developer responsibility to implement this method.
   /// </summary>
   /// <returns>True if the settings are valid, false otherwise.</returns>
   protected abstract ValueTask<bool> AreSettingsValid();

   /// <summary>
   /// If the developer has to override this method, it's important to call the base method because it
   /// fill the fields in the form with the values of the plugin settings.
   /// If the developer doesn't call the base method, the form will be empty and will be the developer
   /// responsibility to fill it.
   /// </summary>
   protected override void OnInitialized()
   {
      base.OnInitialized();
      _state.SetFormFields(PluginSettings);
   }

   /// <summary>
   /// If the developer has to override this method, it's important to call the base method because it
   /// handles the <see cref="IsChanged"/> property properly, resetting the fields only if needed.
   /// If the developer doesn't call the base method, the form will never be updated automatically when
   /// its parameters are set.
   /// </summary>
   protected override void OnParametersSet()
   {
      base.OnParametersSet();
      if (IsChanged)
      {
         _state.SetFormFields(PluginSettings);
      }
   }

   /// <summary>
   /// When the user undo the changes, the form fields are resetted with the original values of the plugin settings.
   /// </summary>
   /// <returns></returns>
   public Task UndoChangesAsync()
   {
      _state.SetFormFields(PluginSettings);
      return Task.CompletedTask;
   }

   async Task<IPluginSettings?> IPluginSettingsTab.SaveAsync()
   {
      var result = await ((IPluginSettingsTab<TPluginSettings>)this).SaveAsync().ConfigureAwait(false);
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);
      return result;
   }

   async Task<TPluginSettings?> IPluginSettingsTab<TPluginSettings>.SaveAsync()
   {
      bool isValid = await AreSettingsValid().ConfigureAwait(false);
      if (!isValid) return null;

      return await PerformSavingAsync().ConfigureAwait(false);
   }

   public abstract class BaseState
   {
      public TPluginSettings? OriginalPluginSettings { get; private set; }

      /// <summary>
      /// Sets the fields in the form with the values of the plugin settings.
      /// DO not use the pluginSettings directly because it can be used as a
      /// way to detect if the settings have been changed.
      /// </summary>
      /// <param name="pluginSettings">The plugin settings.</param>
      public abstract void SetFormFields(TPluginSettings pluginSettings);
   }

   protected readonly TState _state = new();
}

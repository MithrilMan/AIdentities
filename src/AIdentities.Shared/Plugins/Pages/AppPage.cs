using AIdentities.Shared.Services;
using Microsoft.AspNetCore.Components;
using Toolbelt.Blazor.HotKeys2;

namespace AIdentities.Shared.Plugins.Pages;

public abstract class AppPage<TAppComponent> : AppPage<TAppComponent, EmptyAppPageSettings> { }

public abstract class AppPage<TAppComponent, TAppPageSettings> : ComponentBase, IAppPage, IHandleEvent, IDisposable
   where TAppPageSettings : class, IAppComponentSettings, new()
{
   [Inject] protected ILogger<TAppComponent> Logger { get; set; } = default!;
   [Inject] protected IAppComponentSettingsManager ComponentSettingsManager { get; set; } = default!;
   [Inject] protected INotificationService NotificationService { get; set; } = default!;
   [Inject] private HotKeys Hotkeys { get; set; } = default!;

   [Parameter] public string SettingsKey { get; set; } = typeof(TAppPageSettings).Name;

   private readonly CancellationTokenSource _cts = new CancellationTokenSource();

   protected TAppPageSettings? _settings = null;
   protected static bool HasConfiguration => typeof(EmptyAppPageSettings).IsAssignableFrom(typeof(TAppPageSettings)) == false;
   protected bool SettingsLoaded { get; set; } = false;

   private HotKeysContext? _hotKeysContext;

   /// <summary>
   /// Gets the reference to the cancellation token used to signal that the page component has been disposed.
   /// </summary>
   public CancellationToken PageCancellationToken => _cts.Token;


   protected override void OnInitialized()
   {
      Logger.LogTrace(nameof(OnInitialized));
      base.OnInitialized();

      _hotKeysContext = Hotkeys.CreateContext();
      ConfigureHotKeys(_hotKeysContext);

      if (LoadSettings())
      {
         SettingsLoaded = true;
         OnSettingsLoaded();
      }
   }

   protected override Task OnInitializedAsync()
   {
      Logger.LogTrace(nameof(OnInitializedAsync));
      return base.OnInitializedAsync();
   }

   protected override void OnAfterRender(bool firstRender)
   {
      Logger.LogTrace(nameof(OnAfterRender));
      base.OnAfterRender(firstRender);
   }

   protected override Task OnAfterRenderAsync(bool firstRender)
   {
      Logger.LogTrace(nameof(OnAfterRenderAsync));
      return base.OnAfterRenderAsync(firstRender);
   }

   protected override void OnParametersSet()
   {
      Logger.LogTrace(nameof(OnParametersSet));
      base.OnParametersSet();
   }

   protected override Task OnParametersSetAsync()
   {
      Logger.LogTrace(nameof(OnParametersSetAsync));
      return base.OnParametersSetAsync();
   }

   /// <summary>
   /// Override this method to configure custom hotkeys.
   /// </summary>
   /// <param name="hotKeysContext"></param>
   protected virtual void ConfigureHotKeys(HotKeysContext hotKeysContext) { }

   Task IAppComponent.SignalComponentStateHasChanged() => InvokeAsync(StateHasChanged);

   /// <summary>
   /// Called when component settings have been loaded.
   /// </summary>
   protected virtual void OnSettingsLoaded() { }

   protected string GetSettingsKey() => SettingsKey ?? GetType().Name;

   private bool LoadSettings()
   {
      if (!HasConfiguration)
      {
         _settings = (EmptyAppPageSettings.Instance as TAppPageSettings)!;
         return false;
      }

      Logger.LogTrace("Loading for key {SettingsKey}. Setting type: {SettingType}", GetSettingsKey(), typeof(TAppPageSettings).Name);
      var settings = ComponentSettingsManager.GetSettings<TAppPageSettings>(GetSettingsKey());
      if (settings == null)
      {
         Logger.LogTrace("No settings found for key {SettingsKey}. Setting type: {SettingType}", GetSettingsKey(), typeof(TAppPageSettings).Name);
         settings = new TAppPageSettings();
      }
      _settings = settings;

      return true;
   }

   protected async Task<bool> SaveSettings()
   {
      if (!HasConfiguration) return true;
      if (_settings == null) return true;

      if (ComponentSettingsManager.SetSettings(GetSettingsKey(), _settings))
      {
         if (!await ComponentSettingsManager.SaveSettingsAsync(PageCancellationToken).ConfigureAwait(false))
         {
            NotificationService.ShowError("Failed to save component settings.");

            return false;
         }
      }

      return true;
   }

   /// <summary>
   /// Debounces the specified action.
   /// <paramref name="interval"/> is the time to wait before executing the action and is reset each time the action is invoked.
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="actionAsync">The action to debounce.</param>
   /// <param name="interval">
   /// The interval to wait before executing the action.
   /// It is reset each time the action is invoked.
   /// </param>
   /// <returns></returns>
   protected Action<T> DebounceAsync<T>(Func<T, Task> actionAsync, TimeSpan interval)
      => ThrottlerDebouncer.Debounce<T>(async arg =>
   {
      await InvokeAsync(async () =>
      {
         await actionAsync(arg).ConfigureAwait(false);
         StateHasChanged();
      }).ConfigureAwait(false);
   }, interval, PageCancellationToken);

   /// <summary>
   /// Debounces the specified action.
   /// <paramref name="interval"/> is the time to wait before executing the action and is reset each time the action is invoked.
   /// </summary>
   /// <param name="actionAsync">The action to debounce.</param>
   /// <param name="interval">
   /// The interval to wait before executing the action.
   /// It is reset each time the action is invoked.
   /// </param>
   /// <returns></returns>
   protected Action DebounceAsync(Func<Task> actionAsync, TimeSpan interval)
      => ThrottlerDebouncer.Debounce(async () =>
      {
         await InvokeAsync(async () =>
         {
            await actionAsync().ConfigureAwait(false);
            StateHasChanged();
         }).ConfigureAwait(false);
      }, interval, PageCancellationToken);


   /// <summary>
   /// Throttles the specified action.
   /// <paramref name="interval"/> is the minimum time to wait before executing the action.
   /// </summary>
   /// <typeparam name="TArg">The type of the argument passed to the action.</typeparam>
   /// <param name="actionAsync">The action to throttle.</param>
   /// <param name="interval">The minimum time to wait before executing the action.</param>
   /// <returns></returns>
   protected Action<TArg> ThrottleAsync<TArg>(Func<TArg, Task> actionAsync, TimeSpan interval)
      => ThrottlerDebouncer.Throttle<TArg>(async arg =>
      {
         await InvokeAsync(async () =>
         {
            await actionAsync(arg).ConfigureAwait(false);
            StateHasChanged();
         }).ConfigureAwait(false);
      }, interval, PageCancellationToken);

   /// <summary>
   /// Throttles the specified action.
   /// <paramref name="interval"/> is the minimum time to wait before executing the action.
   /// </summary>
   /// <param name="actionAsync">The action to throttle.</param>
   /// <param name="interval">The minimum time to wait before executing the action.</param>
   /// <returns></returns>
   protected Action ThrottleAsync(Func<Task> actionAsync, TimeSpan interval)
      => ThrottlerDebouncer.Throttle(async () =>
      {
         await InvokeAsync(async () =>
         {
            await actionAsync().ConfigureAwait(false);
            StateHasChanged();
         }).ConfigureAwait(false);
      }, interval, PageCancellationToken);

   public virtual void Dispose()
   {
      Logger.LogTrace("Disposing component");
      _cts.Cancel();
      _cts.Dispose();

      _hotKeysContext?.Dispose();

      Logger.LogTrace("Component disposed");
   }
}

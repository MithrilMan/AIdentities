using Microsoft.Extensions.Options;
using MudBlazor;

namespace AIdentities.Shared.Services;

public class NotificationService : INotificationService
{
   const int VISIBLE_DURATION_MILLISECONDS = 10_000;

   private readonly ILogger<NotificationService> _logger;
   readonly AppOptions _options;
   private readonly ISnackbar _snackbar;

   public NotificationService(ILogger<NotificationService> logger, IOptions<AppOptions> options, ISnackbar snackbar)
   {
      _logger = logger;
      _options = options.Value;
      _snackbar = snackbar;

      ConfigureSnackbar();
   }

   private void ConfigureSnackbar()
   {
      _snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
      _snackbar.Configuration.HideTransitionDuration = 1000;
      _snackbar.Configuration.PreventDuplicates = false;
   }

   public void ShowInfo(string message)
   {
      if (_options.Diagnostic.LogNotifications)
      {
         _logger.LogInformation(message);
      }
      _snackbar.Add(message, Severity.Info);
   }

   public void ShowSuccess(string message)
   {
      if (_options.Diagnostic.LogNotifications)
      {
         _logger.LogInformation(message);
      }
      _snackbar.Add(message, Severity.Success);
   }

   public void ShowWarning(string message)
   {
      if (_options.Diagnostic.LogNotifications)
      {
         _logger.LogWarning(message);
      }
      _snackbar.Add(message, Severity.Warning);
   }

   public void ShowError(string message)
   {
      if (_options.Diagnostic.LogNotifications)
      {
         _logger.LogError(message);
      }
      _snackbar.Add(message, Severity.Error, o => o.VisibleStateDuration = VISIBLE_DURATION_MILLISECONDS);
   }

   public void ShowError(Exception ex) => ShowError(ex.Message);
}

namespace AIdentities.Shared.Services;

public interface INotificationService
{
   void ShowError(Exception ex);
   void ShowError(string message);
   void ShowInfo(string message);
   void ShowSuccess(string message);
   void ShowWarning(string message);
}

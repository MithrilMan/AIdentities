namespace AIdentities.UI.Features.Settings.Services;

//create a new exception type
public class InvalidPluginException : Exception
{
   public InvalidPluginException(string message, Exception? innerException = null) : base(message, innerException) { }
}

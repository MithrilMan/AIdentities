namespace AIdentities.Shared.Features.Core.Abstracts;

/// <summary>
/// This is a marker interface used to identify a plugin settings Type.
/// Plugin developers should implement this interface on their plugins if they want to add custom settings
/// to the application settings and have an UI to handle it.
/// Everything could be stored in the ApplicationSettings's Features property, but this interface is used to expose a form
/// within the Settings page, to let the user manage the settings properly instead of having to deal with fils.
/// </summary>
public interface IPluginSettings { }

using AIdentities.Shared.Plugins.Storage;

namespace AIdentities.Shared.Features.Core.Services;

/// <summary>
/// Holds registration about a PluginSetting Type and it's UI Type.
/// Each plugin can define one or more settings Types and UI component that will be exposed
/// in the Settings page.
/// </summary>
/// <param name="pluginStorage">The plugin storage.</param>
/// <param name="PluginSettingType">The plugin setting Type.</param>
/// <param name="PluginSettingTabUIType">The plugin setting UI Type.</param>
/// <param name="UITitle">The UI title that will be shown on the Tab Panel.</param>
public record PluginSettingRegistration(IPluginStorage PluginStorage, Type PluginSettingType, Type PluginSettingTabUIType, string UITitle);

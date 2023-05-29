namespace AIdentities.Shared.Features.Core.Components;

public partial class PluginSettingsSection
{
   /// <summary>
   /// The text to display in the tooltip
   /// </summary>
   [Parameter] public string? TooltipText { get; set; }

   /// <summary>
   /// The title of the section
   /// </summary>
   [Parameter] public string? Title { get; set; }

   /// <summary>
   /// True if the section can be disabled, false otherwise.
   /// If a section can be disabled, it will display a checkbox to enable/disable the section.
   /// </summary>
   [Parameter] public bool CanBeDisabled { get; set; } = false;

   /// <summary>
   /// True if the section is enabled, false otherwise.
   /// </summary>
   [Parameter] public bool IsEnabled { get; set; }

   /// <summary>
   /// Raised when the section is enabled/disabled
   /// </summary>
   [Parameter] public EventCallback<bool> IsEnabledChanged { get; set; }

   /// <summary>
   /// The template to display when the section is enabled
   /// </summary>
   [Parameter] public RenderFragment? EnabledTemplate { get; set; }

   /// <summary>
   /// The template to display when the section is enabled or CanBeDisabled is false.
   /// </summary>
   [Parameter] public RenderFragment? ChildContent { get; set; }

   /// <summary>
   /// The template to display when the section is disabled
   /// </summary>
   [Parameter] public RenderFragment? DisabledTemplate { get; set; }

   bool _isEnabled;
   protected override void OnParametersSet()
   {
      base.OnParametersSet();
      _isEnabled = IsEnabled;
   }

   Task OnSectionEnabledChanged() => IsEnabledChanged.InvokeAsync(_isEnabled);
}

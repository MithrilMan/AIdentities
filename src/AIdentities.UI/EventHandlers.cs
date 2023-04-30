using Microsoft.AspNetCore.Components;

namespace AIdentities.UI;

/// <summary>
/// Exposes handlers for events that are raised by the browser or server.
/// see https://learn.microsoft.com/en-us/aspnet/core/blazor/components/event-handling?view=aspnetcore-7.0#custom-event-arguments
/// </summary>
[EventHandler("ontransitionend", typeof(EventArgs), enableStopPropagation: true, enablePreventDefault: false)]
[EventHandler("onanimationend", typeof(EventArgs), enableStopPropagation: true, enablePreventDefault: false)]
public static class EventHandlers { }

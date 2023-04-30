using Microsoft.AspNetCore.Components;

namespace AIdentities.UI.Features.Automation.Components;

public partial class DutyItem : ComponentBase
{
   [Parameter] public Duty Duty { get; set; } = default!;
}

using AIdentities.Chat.Models;
using Microsoft.AspNetCore.Components;

namespace AIdentities.Chat.Components;

public partial class Message : ComponentBase
{
   [Parameter] public ChatMessage ChatMessage { get; set; } = default!;
}

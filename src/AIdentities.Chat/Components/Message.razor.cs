namespace AIdentities.Chat.Components;

public partial class Message : ComponentBase
{
   [Parameter] public ChatMessage ChatMessage { get; set; } = default!;
}

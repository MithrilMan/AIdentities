namespace AIdentities.Chat.Components;
public partial class TabChatAIdentityFeature
{
   class State
   {
      public bool IsDragging { get; set; } = false;

      public string? Background { get; set; }
      public string? FullPrompt { get; set; }

      public List<AIdentityUserExchange> ExampleMessages { get; set; } = new();

      public bool UseFullPrompt { get; set; } = false;

      internal void SetFormFields(AIdentityChatFeature? chatFeature)
      {
         Background = chatFeature?.Background;
         FullPrompt = chatFeature?.FullPrompt;
         UseFullPrompt = chatFeature?.UseFullPrompt ?? false;
         ExampleMessages = chatFeature?.ExampleMessages ?? new();
      }
   }

   private readonly State _state = new State();
}

namespace AIdentities.Chat.Components;
public partial class TabAIdentityFeatureChat
{
   class State
   {
      public bool IsDragging { get; set; } = false;

      public string? Background { get; set; }
      public string? FullPrompt { get; set; }
      public string? FirstMessage { get; set; }

      public bool UseFullPrompt { get; set; } = false;

      internal void SetFormFields(AIdentityChatFeature? chatFeature)
      {
         Background = chatFeature?.Background;
         FullPrompt = chatFeature?.FullPrompt;
         FirstMessage = chatFeature?.FirstMessage;
         UseFullPrompt = chatFeature?.UseFullPrompt ?? false;
      }
   }

   private readonly State _state = new State();
}

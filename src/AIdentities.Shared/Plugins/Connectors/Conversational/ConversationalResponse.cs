namespace AIdentities.Shared.Plugins.Connectors.Conversational;

public record ConversationalResponse : IConversationalResponse
{
   public string? GeneratedMessage { get; init; }
   public int? PromptTokens { get; set; }
   public int? CompletionTokens { get; init; }
   public int? TotalTokens { get; init; }
   public TimeSpan ResponseTime { get; init; }
   public string? FinishReadon { get; init; }
}

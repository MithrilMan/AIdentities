namespace AIdentities.Shared.Plugins.Connectors.Completion;

public record CompletionResponse : ICompletionResponse
{
   public string? ModelId { get; init; }
   public string? GeneratedMessage { get; init; }
   public int? PromptTokens { get; set; }
   public int? CompletionTokens { get; init; }
   public int? TotalTokens { get; init; }
   public TimeSpan ResponseTime { get; init; }
   public string? FinishReason { get; init; }
}

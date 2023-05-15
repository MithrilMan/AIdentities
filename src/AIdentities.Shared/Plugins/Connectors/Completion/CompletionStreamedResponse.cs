namespace AIdentities.Shared.Plugins.Connectors.Completion;

public class CompletionStreamedResponse : ICompletionStreamedResponse
{
   public string? ModelId { get; init; }
   public string? GeneratedMessage { get; init; }
   public int? PromptTokens { get; set; }
   public int? CumulativeCompletionTokens { get; init; }
   public int? CumulativeTotalTokens { get; init; }
   public TimeSpan CumulativeResponseTime { get; init; }
   public string? FinishReason { get; init; }
}

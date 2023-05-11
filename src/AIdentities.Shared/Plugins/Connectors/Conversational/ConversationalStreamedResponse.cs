namespace AIdentities.Shared.Plugins.Connectors.Conversational;

public class ConversationalStreamedResponse : IConversationalStreamedResponse
{
   public string? GeneratedMessage { get; init; }
   public int? PromptTokens { get; set; }
   public int? CumulativeCompletionTokens { get; init; }
   public int? CumulativeTotalTokens { get; init; }
   public TimeSpan CumulativeResponseTime { get; init; }
   public string? FinishReadon { get; init; }
}

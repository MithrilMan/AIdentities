namespace AIdentities.Shared.Plugins.Connectors.Conversational;
public record DefaultConversationalRequest(AIdentity AIdentity) : IConversationalRequest
{
   public AIdentity AIdentity { get; init; } = AIdentity;
   public string? ModelId { get; init; }
   public IEnumerable<IConversationalMessage> Messages { get; init; } = new List<DefaultConversationalMessage>();
   public int? CompletionResults { get; init; }
   public IList<string>? StopSequences { get; init; }
   public string? UserId { get; init; }
   public decimal? Temperature { get; init; }
   public decimal? RepetitionPenality { get; init; }
   public decimal? RepetitionPenalityRange { get; init; }
   public decimal? TopPSamplings { get; init; }
   public decimal? TopKSamplings { get; init; }
   public decimal? TopASamplings { get; init; }
   public decimal? TypicalSampling { get; init; }
   public decimal? TailFreeSampling { get; init; }
   public int? MaxGeneratedTokens { get; init; }
   public int? ContextSize { get; init; }
   public object? LogitBias { get; init; }
}

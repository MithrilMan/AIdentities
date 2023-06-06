namespace AIdentities.Shared.Plugins.Connectors.Conversational;
public record DefaultConversationalRequest(AIdentity AIdentity) : IConversationalRequest
{
   public AIdentity AIdentity { get; init; } = AIdentity;
   public string? ModelId { get; init; }
   public IEnumerable<IConversationalMessage> Messages { get; init; } = new List<DefaultConversationalMessage>();
   public int? CompletionResults { get; init; }
   public IList<string>? StopSequences { get; init; }
   public string? UserId { get; init; }
   public float? Temperature { get; init; }
   public float? RepetitionPenality { get; init; }
   public float? RepetitionPenalityRange { get; init; }
   public float? TopPSamplings { get; init; }
   public float? TopKSamplings { get; init; }
   public float? TopASamplings { get; init; }
   public float? TypicalSampling { get; init; }
   public float? TailFreeSampling { get; init; }
   public int? MaxGeneratedTokens { get; init; }
   public int? ContextSize { get; init; }
   public object? LogitBias { get; init; }
}

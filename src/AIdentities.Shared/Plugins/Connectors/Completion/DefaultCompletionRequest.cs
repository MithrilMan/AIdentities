namespace AIdentities.Shared.Plugins.Connectors.Completion;
public class DefaultCompletionRequest : ICompletionRequest
{
   public string? ModelId { get; init; }
   public string Prompt { get; init; } = "";
   public string? Suffix { get; init; }
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

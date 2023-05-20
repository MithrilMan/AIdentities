namespace AIdentities.Shared.Features.CognitiveEngine.Models;

public record CognitiveContext
{
   public Dictionary<string, object> StateObjects { get; set; } = new();
}

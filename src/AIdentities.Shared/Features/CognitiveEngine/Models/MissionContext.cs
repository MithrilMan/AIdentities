namespace AIdentities.Shared.Features.CognitiveEngine.Models;

/// <summary>
/// This class is used to pass information between skills execution within a mission context.
/// At any point any AIdentity skill can add or remove information from the context.
/// </summary>
public record MissionContext
{
   public Dictionary<string, object> StateObjects { get; set; } = new();
}

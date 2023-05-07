using AIdentities.Shared.Plugins.Connectors.Conversational;

namespace AIdentities.Chat.Extendability;

public record ChatApiResponse : IConversionalResponse
{
   public string? GeneratedMessage { get; set; }
}

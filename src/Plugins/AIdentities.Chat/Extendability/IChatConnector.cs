namespace AIdentities.Chat.Extendability;

public interface IChatConnector : IEndpointConnector
{
   Task<ChatApiResponse?> SendMessageAsync(ChatApiRequest request);
}

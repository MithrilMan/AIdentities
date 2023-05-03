namespace AIdentities.Chat.Extendability;

public interface IChatConnector : IEndpointConnector
{
   Task<ChatApiResponse?> Request(ChatApiRequest request);
}

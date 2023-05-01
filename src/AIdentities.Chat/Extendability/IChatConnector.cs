namespace AIdentities.Chat.Extendability;

public interface IChatConnector : IEndpointConnector
{
   IChatApiRequest Settings { get; }

   Task<ChatApiResponse?> Request(ChatApiRequest request);
}

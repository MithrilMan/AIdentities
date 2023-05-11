using AIdentities.Shared.Plugins.Connectors.Conversational;

namespace AIdentities.Chat.Services;
public interface IChatPromptGenerator
{
   /// <summary>
   /// Sets the current conversation to generate a prompt for.
   /// </summary>
   /// <param name="conversation">The conversation to generate a prompt for.</param>
   void InitializeConversation(Conversation? conversation);

   /// <summary>
   /// Add a message to the conversation history.
   /// </summary>
   /// <param name="message"></param>
   void AppendMessage(ChatMessage message);

   /// <summary>
   /// Generate an API request for the current conversation.
   /// </summary>
   /// <param name="conversation"></param>
   /// <returns></returns>
   Task<IConversationalRequest> GenerateApiRequest();
}

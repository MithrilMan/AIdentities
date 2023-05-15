namespace AIdentities.Shared.Plugins.Connectors.Conversational;

/// <summary>
/// To be used for endpoints that support conversations.
/// It represents a message to be sent to the endpoint.
/// </summary>
public interface IConversationalMessage
{
   /// <summary>
   /// The role of the author of this message.
   /// </summary>
   DefaultConversationalRole Role { get; init; }

   /// <summary>
   /// The contents of the message
   /// </summary>
   string Content { get; init; }

   /// <summary>
   /// The name of the user in a multi-user chat.
   /// </summary>
   string? Name { get; init; }
}

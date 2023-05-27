namespace AIdentities.Chat.Services;

/// <summary>
/// Allows to export a conversation to a file.
/// </summary>
public interface IChatExporter
{
   /// <summary>
   /// Exports the conversation with the given id to a file.
   /// </summary>
   /// <param name="conversationId">The id of the conversation to export.</param>
   /// <param name="format">The format to export the conversation to.</param>
   Task ExportConversationAsync(Guid conversationId, ConversationExportFormat format);
}

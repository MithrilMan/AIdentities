namespace AIdentities.Chat.Services;

public class ChatStorage : IChatStorage
{
   const string CONVERSATION_POSTFIX = ".conv.json";
   const string CONVERSATION_MESSAGES_POSTFIX = ".conv.messages";

   readonly ILogger<ChatStorage> _logger;
   readonly IPluginStorage<PluginEntry> _pluginStorage;

   public ChatStorage(ILogger<ChatStorage> logger, IPluginStorage<PluginEntry> pluginStorage)
   {
      _logger = logger;
      _pluginStorage = pluginStorage;
   }

   public ValueTask<bool> DeleteConversationAsync(Guid conversationId)
   {
      _pluginStorage.DeleteAsync($"{conversationId}{CONVERSATION_POSTFIX}");
      _pluginStorage.DeleteAsync($"{conversationId}{CONVERSATION_MESSAGES_POSTFIX}");
      return new ValueTask<bool>(true);
   }

   public async ValueTask<IEnumerable<ConversationMetadata>> GetConversationsAsync()
   {
      var files = await _pluginStorage.ListAsync().ConfigureAwait(false);

      var conversations = new List<ConversationMetadata>();
      foreach (var file in files.Where(f => f.EndsWith(CONVERSATION_POSTFIX)))
      {
         var conversation = await _pluginStorage.ReadAsJsonAsync<ConversationMetadata>(file).ConfigureAwait(false);
         conversations.Add(conversation!);
      }
      return conversations;
   }

   public async ValueTask<IEnumerable<ConversationMetadata>> GetConversationsByAIdentityAsync(AIdentity aIdentity)
   {
      var files = await _pluginStorage.ListAsync().ConfigureAwait(false);

      var conversations = new List<ConversationMetadata>();
      foreach (var file in files)
      {
         if (file.EndsWith(CONVERSATION_POSTFIX)) continue;

         //search if file contains the AIdentity id
         var conversation = await _pluginStorage.ReadAsJsonAsync<ConversationMetadata>(file).ConfigureAwait(false);
         if (conversation?.AIdentityId == aIdentity.Id)
         {
            conversations.Add(conversation);
         }
      }
      return conversations;
   }

   public async ValueTask<Conversation> LoadConversationAsync(Guid conversationId)
   {
      var fileName = $"{conversationId}{CONVERSATION_POSTFIX}";
      var conversationMetadata = await _pluginStorage.ReadAsJsonAsync<ConversationMetadata>(fileName).ConfigureAwait(false)
         ?? throw new ArgumentException($"Conversation with id {conversationId} not found.");

      var conversation = new Conversation
      {
         Id = conversationMetadata.ConversationId,
         Metadata = conversationMetadata,
         Messages = new List<ChatMessage>()
      };

      var messagesFileName = GetConversationMessagesFileName(conversationId);
      try
      {
         var messages = await _pluginStorage.ReadAsync(messagesFileName).ConfigureAwait(false);

         if (string.IsNullOrWhiteSpace(messages))
         {
            return conversation;
         }

         //each line is a json formatted message
         foreach (var message in messages.Split('\n'))
         {
            if (string.IsNullOrWhiteSpace(message))
            {
               continue;
            }
            var chatMessage = JsonSerializer.Deserialize<ChatMessage>(message);
            conversation.Messages.Add(chatMessage!);
         }
      }
      catch (Exception ex)
      {
         _logger.LogError(ex, "Conversation with id {conversationId} is corrupted.", conversationId);
         throw new FormatException($"Conversation with id {conversationId} is corrupted.", ex);
      }

      return conversation!;
   }

   private static string GetConversationMessagesFileName(Guid conversationId) => $"{conversationId}{CONVERSATION_MESSAGES_POSTFIX}";

   public async ValueTask<bool> UpdateConversationAsync(ConversationMetadata conversationMetadata, ChatMessage? message)
   {
      var fileName = $"{conversationMetadata.ConversationId}{CONVERSATION_POSTFIX}";
      conversationMetadata.UpdatedAt = DateTimeOffset.UtcNow;

      if (message != null)
      {
         var messagesFileName = $"{conversationMetadata.ConversationId}{CONVERSATION_MESSAGES_POSTFIX}";
         await _pluginStorage.AppendAsync(messagesFileName, JsonSerializer.Serialize(message) + Environment.NewLine).ConfigureAwait(false);
         conversationMetadata.MessageCount++;
      }

      await _pluginStorage.WriteAsJsonAsync(fileName, conversationMetadata).ConfigureAwait(false);

      return true;
   }

   public static void RemoveLineFromFile(string filePath, int lineIndexToRemove)
   {
      string[] lines = File.ReadAllLines(filePath);

      if (lineIndexToRemove < 0 || lineIndexToRemove >= lines.Length)
      {
         throw new ArgumentOutOfRangeException(nameof(lineIndexToRemove));
      }
   }

   public async ValueTask<bool> DeleteMessageAsync(ConversationMetadata conversationMetadata, ChatMessage message)
   {
      var fileName = $"{conversationMetadata.ConversationId}{CONVERSATION_POSTFIX}";
      conversationMetadata.UpdatedAt = DateTimeOffset.UtcNow;

      var messagesFileName = GetConversationMessagesFileName(conversationMetadata.ConversationId);

      var originalMessages = (await _pluginStorage.ReadAllLinesAsync(messagesFileName).ConfigureAwait(false))?.ToList();
      if (originalMessages is not { Count: > 0 }) return false;

      var textToFind = message.Id.ToString();
      // compact the file removing the found line and empty lines
      var remainingLines = originalMessages.Where(line => !line.Contains(textToFind) && !string.IsNullOrWhiteSpace(line));

      await _pluginStorage.WriteAllLinesAsync(
         messagesFileName,
         remainingLines
         ).ConfigureAwait(false);

      //we rewrite whole file (we need a better storage, maybe sqlite)
      var messageCount = remainingLines.Count();
      int deletedMessage = conversationMetadata.MessageCount - messageCount;

      //se il numero di messaggi non è uguale, aggiorniamo il file di metadati
      if (deletedMessage != 0)
      {
         conversationMetadata.MessageCount = messageCount;
         await _pluginStorage.WriteAsJsonAsync(fileName, conversationMetadata).ConfigureAwait(false);
      }

      return deletedMessage > 0;
   }

   public ValueTask StartConversationAsync(Conversation conversation)
   {
      if (conversation is null or { Metadata: null })
      {
         throw new ArgumentException("Conversation or conversation metadata cannot be null.");
      }

      var fileName = $"{conversation.Id}{CONVERSATION_POSTFIX}";
      _pluginStorage.WriteAsJsonAsync(fileName, conversation.Metadata);

      var messagesFileName = $"{conversation.Id}{CONVERSATION_MESSAGES_POSTFIX}";

      if (conversation.Messages is null or { Count: 0 })
      {
         _pluginStorage.WriteAsync(messagesFileName, string.Empty);
         return ValueTask.CompletedTask;
      }

      var messages = string.Join(Environment.NewLine, conversation.Messages.Select(message => JsonSerializer.Serialize(message)));
      return ValueTask.CompletedTask;
   }
}

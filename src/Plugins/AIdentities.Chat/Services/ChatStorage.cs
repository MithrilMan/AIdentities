namespace AIdentities.Chat.Services;
public class ChatStorage : IChatStorage
{
   const string CONVERSATION_POSTFIX = ".conv.json";
   const string CONVERSATION_MESSAGES_POSTFIX = ".conv.messages";

   readonly ILogger<ChatStorage> _logger;
   readonly IPluginStorage _pluginStorage;

   public ChatStorage(ILogger<ChatStorage> logger, IPluginStorage pluginStorage)
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
      var conversations = new List<ConversationMetadata>();
      var files = _pluginStorage.ListAsync().Result;
      foreach (var file in files.Where(f => f.EndsWith(CONVERSATION_POSTFIX)))
      {
         var conversation = await _pluginStorage.ReadAsJsonAsync<ConversationMetadata>(file).ConfigureAwait(false);
         conversations.Add(conversation!);
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

      var messagesFileName = $"{conversationId}{CONVERSATION_MESSAGES_POSTFIX}";
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
         _logger.LogError(ex, $"Conversation with id {conversationId} is corrupted.");
         throw new FormatException($"Conversation with id {conversationId} is corrupted.", ex);
      }

      return conversation!;
   }

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

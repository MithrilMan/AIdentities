using AIdentities.Shared.Features.CognitiveEngine.Memory.Conversation;

namespace AIdentities.Chat.Services;

public class CognitiveChatStorage : ICognitiveChatStorage
{
   const string CONVERSATION_POSTFIX = ".conversation.json";
   const string CONVERSATION_MESSAGES_POSTFIX = ".conversation.messages";

   readonly ILogger<CognitiveChatStorage> _logger;
   readonly IPluginStorage<PluginEntry> _pluginStorage;

   public CognitiveChatStorage(ILogger<CognitiveChatStorage> logger, IPluginStorage<PluginEntry> pluginStorage)
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
      foreach (var fileName in files.Where(f => f.EndsWith(CONVERSATION_POSTFIX)))
      {
         var conversation = await LoadConversationMetadata(fileName).ConfigureAwait(false);
         conversations.Add(conversation!);
      }
      return conversations;
   }

   public async ValueTask<IEnumerable<ConversationMetadata>> GetConversationsByAIdentityAsync(AIdentity aIdentity)
   {
      var files = await _pluginStorage.ListAsync().ConfigureAwait(false);

      var conversations = new List<ConversationMetadata>();
      foreach (var fileName in files)
      {
         if (fileName.EndsWith(CONVERSATION_POSTFIX)) continue;

         //search if file contains the AIdentity id
         var conversation = await LoadConversationMetadata(fileName).ConfigureAwait(false);
         if (conversation!.AIdentityIds.Contains(aIdentity.Id))
         {
            conversations.Add(conversation);
         }
      }
      return conversations;
   }

   public async ValueTask<Conversation> LoadConversationAsync(Guid conversationId)
   {
      var fileName = $"{conversationId}{CONVERSATION_POSTFIX}";
      var conversationMetadata = await LoadConversationMetadata(fileName).ConfigureAwait(false);

      var conversation = new Conversation
      {
         Id = conversationMetadata.ConversationId,
         Metadata = conversationMetadata
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
            var ConversationMessage = JsonSerializer.Deserialize<ConversationMessage>(message);
            conversation.AddMessage(ConversationMessage!);
         }
      }
      catch (Exception ex)
      {
         _logger.LogError(ex, "Conversation with id {conversationId} is corrupted.", conversationId);
         throw new FormatException($"Conversation with id {conversationId} is corrupted.", ex);
      }

      return conversation!;
   }

   private async Task<ConversationMetadata> LoadConversationMetadata(string fileName)
   {
      var conversationMetadata = await _pluginStorage.ReadAsJsonAsync<ConversationMetadata>(fileName).ConfigureAwait(false)
            ?? throw new ArgumentException($"Conversation with file name {fileName} not found.");

      //if I don't have any AIdentityIds it's because I'm on the old version that had just a single AIdentityId conversation.
      //we try to load it anyway and convert to the new format
      if (conversationMetadata.AIdentityIds is { Count: > 0 })
      {
         return conversationMetadata;
      }

      var oldConversationMetadata = await _pluginStorage.ReadAsJsonAsync<ConversationMetadataOld>(fileName).ConfigureAwait(false);
      if (oldConversationMetadata?.AIdentityId != null)
      {
         oldConversationMetadata.AIdentityIds.Add(oldConversationMetadata.AIdentityId.Value);
         return oldConversationMetadata;
      }

      return oldConversationMetadata!;
   }

   private static string GetConversationMessagesFileName(Guid conversationId) => $"{conversationId}{CONVERSATION_MESSAGES_POSTFIX}";

   public async ValueTask<bool> UpdateConversationAsync(ConversationMetadata conversationMetadata, ConversationMessage? message)
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

   public async ValueTask<bool> DeleteMessageAsync(ConversationMetadata conversationMetadata, ConversationMessage message)
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


   public record ConversationMetadataOld : ConversationMetadata
   {
      public Guid? AIdentityId { get; set; }
   }
}

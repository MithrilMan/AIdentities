using MudBlazor.Extensions;

namespace AIdentities.Chat.Services;
public class ChatAIdentitySafetyChecker : IAIdentitySafetyChecker
{

   readonly ILogger<ChatAIdentitySafetyChecker> _logger;
   readonly IChatStorage _chatStorage;

   public ChatAIdentitySafetyChecker(ILogger<ChatAIdentitySafetyChecker> logger, IChatStorage chatStorage)
   {
      _logger = logger;
      _chatStorage = chatStorage;
   }

   public async ValueTask<AIdentityPluginActivity?> GetAIdentityActivityAsync(AIdentity aIdentity)
   {
      var result = new ChatAIdentityPluginActivity();

      int convestationsCount = 0;
      var conversations = await _chatStorage.GetConversationsAsync().ConfigureAwait(false);
      foreach (var metadata in conversations)
      {
         // increment the number of conversations the AIdentity has had
         if (metadata.AIdentityIds.Contains(aIdentity.Id)) convestationsCount++;
      }

      result.Conversations = new(convestationsCount, $"{convestationsCount} conversations");

      return result;
   }

   public async ValueTask<(bool canDelete, string? reasonToNotDelete)> IsAIdentitySafeToBeDeletedAsync(AIdentity aIdentity)
   {
      ChatAIdentityPluginActivity activities = (await GetAIdentityActivityAsync(aIdentity).ConfigureAwait(false)).As<ChatAIdentityPluginActivity>();

      if (activities.Conversations.Count > 0)
      {
         return (false, $"AIdentity has {activities.Count} activities, deleting it would could cause data loss.");
      }

      return (true, null);
   }
}

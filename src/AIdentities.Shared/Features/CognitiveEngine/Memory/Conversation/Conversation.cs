using System.Diagnostics.CodeAnalysis;

namespace AIdentities.Shared.Features.CognitiveEngine.Memory.Conversation;

/// <summary>
/// Represents a full conversation.
/// It contains the metadata of the conversation and the messages.
/// </summary>
public class Conversation
{
   /// <summary>
   /// The unique identifier of the conversation.
   /// </summary>
   public Guid Id { get; init; } = Guid.NewGuid();

   /// <summary>
   /// The metadata of the conversation.
   /// It contains statistics about the conversation and the participants.
   /// </summary>
   public ConversationMetadata Metadata { get; init; } = default!;

   /// <summary>
   /// The messages of the conversation.
   /// </summary>
   private List<ConversationMessage> _messages { get; init; } = new();
   /// <summary>
   /// The messages of the conversation.
   /// </summary>
   public IReadOnlyCollection<ConversationMessage> Messages => _messages;

   /// <summary>
   /// Allows to add a message to the conversation and update the statistics.
   /// </summary>
   /// <param name="message"></param>
   public void AddMessage(ConversationMessage message)
   {
      _messages.Add(message);
      Metadata.MessageCount++;
      Metadata.UpdatedAt = DateTimeOffset.UtcNow;
   }

   /// <summary>
   /// Allows to remove a message from the conversation and update the statistics.
   /// </summary>
   /// <param name="messageId"></param>
   public bool RemoveMessage(Guid messageId)
   {
      var message = Messages.FirstOrDefault(x => x.Id == messageId);
      if (message is null)
      {
         return false;
      }

      _messages.Remove(message);
      Metadata.MessageCount--;
      Metadata.UpdatedAt = DateTimeOffset.UtcNow;
      return true;
   }

   /// <summary>
   /// Adds an AIdentity to the conversation.
   /// </summary>
   /// <param name="aIdentity">The AIdentity to add.</param>
   /// <returns>True if the AIdentity was added, false if it was already present.</returns>
   public bool AddAIdentity(AIdentity aIdentity)
   {
      if (Metadata.AIdentityIds.Contains(aIdentity.Id))
      {
         return false;
      }

      Metadata.AIdentityIds.Add(aIdentity.Id);
      Metadata.UpdatedAt = DateTimeOffset.UtcNow;
      return true;
   }


   /// <summary>
   /// Tries to remove an AIdentity from the conversation.
   /// If the AIdentity is still present in the conversation, it will fail.
   /// </summary>
   /// <param name="aIdentity">The AIdentity to remove.</param>
   /// <param name="errorReason">The reason why the AIdentity could not be removed.</param>
   /// <returns>True if the AIdentity was removed, false if it was still present in the conversation.</returns>
   public bool TryRemoveAIdentity(AIdentity aIdentity, [MaybeNullWhen(true)] out string errorReason)
      => RemoveAuthor(aIdentity.Id, true, out errorReason);

   /// <summary>
   /// Adds a human to the conversation.
   /// </summary>
   /// <param name="humanId">The Id of the human to add.</param>
   /// <returns>True if the human was added, false if it was already present.</returns>
   public bool AddHuman(Guid humanId)
   {
      if (Metadata.Humans.Contains(humanId))
      {
         return false;
      }

      Metadata.Humans.Add(humanId);
      Metadata.UpdatedAt = DateTimeOffset.UtcNow;
      return true;
   }

   /// <summary>
   /// Tries to remove a human from the conversation.
   /// </summary>
   /// <param name="humanId">The Id of the human to remove.</param>
   /// <param name="errorReason">The reason why the human could not be removed.</param>
   /// <returns>True if the human was removed, false if it was still present in the conversation.</returns>
   public bool TryRemoveHuman(Guid humanId, [MaybeNullWhen(true)] out string errorReason)
      => RemoveAuthor(humanId, false, out errorReason);


   private bool RemoveAuthor(Guid authorId, bool isAiGenerated, [MaybeNullWhen(true)] out string errorReason)
   {
      if (Messages.Any(x => x.IsAIGenerated == isAiGenerated && x.AuthorId == authorId))
      {
         errorReason = "The author is still present in the conversation.";
         return false;
      }

      bool hasRemoved;
      if (isAiGenerated)
      {
         hasRemoved = Metadata.AIdentityIds.Remove(authorId);
      }
      else
      {
         hasRemoved = Metadata.Humans.Remove(authorId);
      }

      if (!hasRemoved)
      {
         errorReason = "The author was not present in the conversation.";
         return false;
      }

      Metadata.UpdatedAt = DateTimeOffset.UtcNow;

      errorReason = null;
      return true;
   }
}

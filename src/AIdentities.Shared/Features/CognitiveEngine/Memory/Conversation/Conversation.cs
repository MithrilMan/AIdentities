using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http.Features;

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
   public Guid Id { get; private set; }

   /// <summary>
   /// The date and time the conversation was created.
   /// </summary>
   public DateTimeOffset CreatedAt { get; private set; }

   private readonly HashSet<Guid> _humans = new();
   /// <summary>
   /// Contains a list of human Ids that are part of this conversation.
   /// It's technically an hashset so we don't have to bother with duplicates.
   /// </summary>
   public IReadOnlyCollection<Guid> Humans => _humans;

   private readonly HashSet<Guid> _aIdentityIds = new();
   /// <summary>
   /// Contains a list of AIdentity Ids that are part of this conversation.
   /// It's technically an hashset so we don't have to bother with duplicates.
   /// </summary>
   public IReadOnlyCollection<Guid> AIdentityIds => _aIdentityIds;

   /// <summary>
   /// The title of the conversation.
   /// </summary>
   public string Title { get; private set; } = default!;

   /// <summary>
   /// The date and time the conversation was last updated.
   /// </summary>
   public DateTimeOffset UpdatedAt { get; private set; }

   /// <summary>
   /// The number of messages in the conversation.
   /// </summary>
   public int MessageCount { get; private set; }

   private readonly List<ConversationMessage> _messages = new();
   /// <summary>
   /// The messages of the conversation.
   /// </summary>
   public virtual IReadOnlyCollection<ConversationMessage> Messages => _messages;

   /// <summary>
   /// An open door to ORM that can take advantage of this constructor (e.g. EF Core).
   /// </summary>
   protected Conversation() { }

   public Conversation(string title)
   {
      Id = Guid.NewGuid();
      Title = title;
      CreatedAt = UpdatedAt = DateTimeOffset.UtcNow;
   }

   /// <summary>
   /// Allows to add a message to the conversation and update the statistics.
   /// </summary>
   /// <param name="message"></param>
   public void AddMessage(ConversationMessage message)
   {
      _messages.Add(message);
      MessageCount++;
      UpdatedAt = DateTimeOffset.UtcNow;
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
      MessageCount--;
      UpdatedAt = DateTimeOffset.UtcNow;
      return true;
   }

   /// <summary>
   /// Adds an AIdentity to the conversation.
   /// </summary>
   /// <param name="aIdentity">The AIdentity to add.</param>
   /// <returns>True if the AIdentity was added, false if it was already present.</returns>
   public bool AddAIdentity(AIdentity aIdentity)
   {
      if (_aIdentityIds.Contains(aIdentity.Id))
      {
         return false;
      }

      _aIdentityIds.Add(aIdentity.Id);
      UpdatedAt = DateTimeOffset.UtcNow;
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
      if (_humans.Contains(humanId))
      {
         return false;
      }

      _humans.Add(humanId);
      UpdatedAt = DateTimeOffset.UtcNow;
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
         hasRemoved = _aIdentityIds.Remove(authorId);
      }
      else
      {
         hasRemoved = _humans.Remove(authorId);
      }

      if (!hasRemoved)
      {
         errorReason = "The author was not present in the conversation.";
         return false;
      }

      UpdatedAt = DateTimeOffset.UtcNow;

      errorReason = null;
      return true;
   }

   /// <summary>
   /// Updates the title of the conversation.
   /// </summary>
   /// <param name="title">The new title of the conversation.</param>
   public void UpdateTitle(string title)
   {
      Title = title;
      UpdatedAt = DateTimeOffset.UtcNow;
   }
}

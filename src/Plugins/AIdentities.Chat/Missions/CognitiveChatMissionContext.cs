namespace AIdentities.Chat.Missions;

public class CognitiveChatMissionContext : MissionContext
{
   /// <summary>
   /// The current conversation
   /// </summary>
   public Conversation? CurrentConversation
   {
      get => GetOrDefault<Conversation>(nameof(CurrentConversation));
      set => SetOrRemove(nameof(CurrentConversation), value);
   }

   /// <summary>
   /// The current conversation history.
   /// </summary>
   public IConversationHistory? ConversationHistory
   {
      get => GetOrDefault<IConversationHistory>(nameof(ConversationHistory));
      set => SetOrRemove(nameof(ConversationHistory), value);
   }

   /// <summary>
   /// The list of AIdentities that are participating to the conversation
   /// </summary>
   public Dictionary<Guid, ParticipatingAIdentity> ParticipatingAIdentities
   {
      get => GetOrDefault<Dictionary<Guid, ParticipatingAIdentity>>(nameof(ParticipatingAIdentities), new());
      set => State[nameof(ParticipatingAIdentities)] = value;
   }

   /// <summary>
   /// True if the conversation is moderated, false otherwise
   /// </summary>
   public bool IsModeratedModeEnabled
   {
      get => GetOrDefault<bool>(nameof(IsModeratedModeEnabled));
      set => SetOrRemove(nameof(IsModeratedModeEnabled), value);
   }

   /// <summary>
   /// The next talker that should reply to the current message
   /// </summary>
   public AIdentity? NextTalker
   {
      get => GetOrDefault<AIdentity?>(nameof(NextTalker));
      set => SetOrRemove(nameof(NextTalker), value);
   }

   /// <summary>
   /// Sets the message to reply to.
   /// When this property is set, the <see cref="NextTalker"/> that message history will stop at this message.
   /// Once the message has been used, this property should be reset to null.
   /// </summary>
   public ConversationMessage? MessageToReplyTo
   {
      get => GetOrDefault<ConversationMessage?>(nameof(MessageToReplyTo));
      set => SetOrRemove(nameof(MessageToReplyTo), value);
   }
}

namespace AIdentities.Chat.Missions;

public class CognitiveChatMissionContext : MissionContext
{
   /// <summary>
   /// The current conversation
   /// </summary>
   public Conversation? CurrentConversation
   {
      get => GetOrDefault<Conversation?>(nameof(CurrentConversation), null);
      set => SetOrRemove(nameof(CurrentConversation), value);
   }

   /// <summary>
   /// The current conversation history.
   /// </summary>
   public IConversationHistory? ConversationHistory
   {
      get => GetOrDefault<IConversationHistory?>(nameof(ConversationHistory), null);
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
      get => GetOrDefault<bool>(nameof(IsModeratedModeEnabled), false);
      set => SetOrRemove(nameof(IsModeratedModeEnabled), value);
   }

   /// <summary>
   /// The next talker that should reply to the current message
   /// </summary>
   public AIdentity? NextTalker
   {
      get => GetOrDefault<AIdentity?>(nameof(NextTalker), null);
      set => SetOrRemove(nameof(NextTalker), value);
   }

   /// <summary>
   /// Sets the message to reply to.
   /// When this property is set, the <see cref="NextTalker"/> that message history will stop at this message.
   /// Once the message has been used, this property should be reset to null.
   /// </summary>
   public ConversationMessage? MessageToReplyTo
   {
      get => GetOrDefault<ConversationMessage?>(nameof(MessageToReplyTo), null);
      set => SetOrRemove(nameof(MessageToReplyTo), value);
   }
}

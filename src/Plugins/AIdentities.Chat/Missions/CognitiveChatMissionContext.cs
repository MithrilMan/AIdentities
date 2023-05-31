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
   /// The current conversation history
   /// </summary>
   public IConversationHistory? ConversationHistory
   {
      get => GetOrDefault<IConversationHistory>(nameof(IConversationHistory));
      set => SetOrRemove(nameof(IConversationHistory), value);
   }

   /// <summary>
   /// The list of AIdentities that are partecipating to the conversation
   /// </summary>
   public Dictionary<Guid, PartecipatingAIdentity> PartecipatingAIdentities
   {
      get => GetOrDefault<Dictionary<Guid, PartecipatingAIdentity>>(nameof(PartecipatingAIdentities), new());
      set => State[nameof(PartecipatingAIdentities)] = value;
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
}

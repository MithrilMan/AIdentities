namespace AIdentities.Chat.Missions;
public class CognitiveChatMissionContext : MissionContext
{
   public Conversation? CurrentConversation
   {
      get => GetOrDefault<Conversation>(nameof(CurrentConversation));
      set => SetOrRemove(nameof(CurrentConversation), value);
   }

   public Dictionary<Guid, PartecipatingAIdentity> PartecipatingAIdentities
   {
      get => GetOrDefault<Dictionary<Guid, PartecipatingAIdentity>>(nameof(PartecipatingAIdentities), new());
      set => State[nameof(PartecipatingAIdentities)] = value;
   }
}

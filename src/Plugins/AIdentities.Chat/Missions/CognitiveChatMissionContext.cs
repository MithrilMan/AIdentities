using AIdentities.Shared.Features.CognitiveEngine.Mission;

namespace AIdentities.Chat.Missions;
public class CognitiveChatMissionContext : MissionContext
{
   public Conversation? CurrentConversation
   {
      get => GetOrDefault<Conversation>(nameof(CurrentConversation));
      set => SetOrRemove(nameof(CurrentConversation), value);
   }

   public HashSet<AIdentity> PartecipatingAIdentities
   {
      get => GetOrDefault<HashSet<AIdentity>>(nameof(PartecipatingAIdentities), new());
      set => State[nameof(PartecipatingAIdentities)] = value;
   }
}

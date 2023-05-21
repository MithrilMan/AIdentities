using AIdentities.Shared.Features.CognitiveEngine.Skills;

namespace AIdentities.Chat.Skills.InviteFriend;

record Args
{
   public static SkillArgumentDefinition WhoToInviteDefinition
      = new SkillArgumentDefinition(nameof(WhoToInvite), "Who the user wants to talk with / invite.", true);

   public static SkillArgumentDefinition CharacteristicToHaveDefinition
      = new SkillArgumentDefinition(nameof(CharacteristicToHave), "Which characteristic the friend must have in order to be invited.", true);


   public string? WhoToInvite { get; set; }

   public string? CharacteristicToHave { get; set; }
}

using AIdentities.Shared.Features.CognitiveEngine.Skills;

namespace AIdentities.Chat.Skills.IntroduceAIdentity;

record Args
{
   public static SkillArgumentDefinition AIdentityIdDefinition
      = new SkillArgumentDefinition(nameof(AIdentityId), "Who is the AIdentity that's joining the conversation.", true);


   public string AIdentityId { get; set; } = default!;
}

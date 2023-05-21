namespace AIdentities.Shared.Features.CognitiveEngine.Skills;

public class SkillActionsManager : ISkillActionsManager
{
   readonly ILogger<SkillActionsManager> _logger;
   public IEnumerable<ISkillAction> AvailableSkills { get; }


   public SkillActionsManager(ILogger<SkillActionsManager> logger, IEnumerable<ISkillAction> availableSkills)
   {
      _logger = logger;
      AvailableSkills = availableSkills;
   }

   public ISkillAction? Get(string skillName)
   {
      return AvailableSkills
         .FirstOrDefault(s => StringComparer.InvariantCultureIgnoreCase.Equals(s.Name, skillName));
   }
}

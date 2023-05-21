namespace AIdentities.Shared.Features.CognitiveEngine.Skills;

public class SkillManager : ISkillManager
{
   readonly ILogger<SkillManager> _logger;
   readonly Dictionary<string, ISkillAction> _knownSkills;

   public SkillManager(ILogger<SkillManager> logger, IEnumerable<ISkillAction> availableSkills)
   {
      _logger = logger;
      _knownSkills = availableSkills.ToDictionary(s => s.Name, s => s);
   }

   public ISkillAction? Get(string skillName)
   {
      if (_knownSkills.TryGetValue(skillName, out var skill))
      {
         return skill;
      }

      _logger.LogDebug("Skill {SkillName} not found.", skillName);
      return null;
   }

   public IEnumerable<ISkillAction> All() => _knownSkills.Values;
}

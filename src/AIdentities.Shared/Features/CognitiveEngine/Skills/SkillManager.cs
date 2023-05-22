namespace AIdentities.Shared.Features.CognitiveEngine.Skills;

public class SkillManager : ISkillManager
{
   readonly ILogger<SkillManager> _logger;
   readonly Dictionary<string, ISkill> _knownSkills;

   public SkillManager(ILogger<SkillManager> logger, IEnumerable<ISkill> availableSkills)
   {
      _logger = logger;
      _knownSkills = availableSkills.ToDictionary(s => s.Name, s => s);
   }

   public ISkill? Get(string skillName)
   {
      if (_knownSkills.TryGetValue(skillName, out var skill))
      {
         return skill;
      }

      _logger.LogDebug("Skill {SkillName} not found.", skillName);
      return null;
   }

   public IEnumerable<ISkill> All() => _knownSkills.Values;

   public ISkill? Get<TSkill>() => _knownSkills.Values.FirstOrDefault(s => s.GetType() == typeof(TSkill));
}

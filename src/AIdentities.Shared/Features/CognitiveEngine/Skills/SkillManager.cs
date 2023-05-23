namespace AIdentities.Shared.Features.CognitiveEngine.Skills;

public class SkillManager : ISkillManager
{
   readonly ILogger<SkillManager> _logger;
   readonly Dictionary<string, ISkill> _knownSkills = new();
   readonly Dictionary<Type, SkillDefinition> _skillsDefinitions = new();

   public SkillManager(ILogger<SkillManager> logger, IEnumerable<ISkill> availableSkills)
   {
      _logger = logger;

      BuildSkillDefinitions(availableSkills);
   }

   private void BuildSkillDefinitions(IEnumerable<ISkill> availableSkills)
   {
      foreach (var skill in availableSkills)
      {
         var skillType = skill.GetType();
         try
         {
            var skillDefinition = SkillDefinitionBuilder.BuildSkillDefinition(skillType);
            _knownSkills[skillDefinition.Name] = skill;
            skill.Definition = skillDefinition;
            _skillsDefinitions[skillType] = skillDefinition;
         }
         catch (Exception ex)
         {
            _logger.LogError(
               ex,
               """
               The skill {skillTypeName} doesn't contains a valid skill definition.
               Ensure the proper attributes have been used to define the skill.
               The skill will be ignored and will not be available.
               """,
               skillType.Name
               );
         }
      }
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

   /// <inheritdoc />
   public SkillDefinition? GetSkillDefinition(Type skillType)
   {
      if (_skillsDefinitions.TryGetValue(skillType, out var skillDefinition))
      {
         return skillDefinition;
      }

      _logger.LogDebug("Skill definition for {SkillType} not found.", skillType.Name);
      return null;
   }

   /// <inheritdoc />
   public SkillDefinition? GetSkillDefinition<TSkill>() where TSkill : ISkill => GetSkillDefinition(typeof(TSkill));

   /// <inheritdoc />
   public SkillDefinition? GetSkillDefinition(string skillName)
   {
      var skill = Get(skillName);
      if (skill is null)
      {
         _logger.LogDebug("Skill definition for {SkillName} not found.", skillName);
         return null;
      }

      return GetSkillDefinition(skill.GetType());
   }

   /// <inheritdoc />
   public IEnumerable<SkillDefinition> GetSkillDefinitions() => _skillsDefinitions.Values;
}

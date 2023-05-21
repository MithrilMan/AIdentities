using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIdentities.Shared.Features.CognitiveEngine.Skills;
public class SkillManager
{
   readonly ILogger<SkillManager> _logger;
   readonly Dictionary<Type, SkillDefinition> _registeredSkills = new();

   public SkillManager(ILogger<SkillManager> logger, IEnumerable<ISkillAction> registeredSkills)
   {
      _logger = logger;

      RegisterSkills();
   }

   void RegisterSkills()
   {

   }
}

namespace AIdentities.Shared.Features.CognitiveEngine.Skills;

public interface ISkillActionsManager
{
   /// <summary>
   /// The list of available skills.
   /// </summary>
   IEnumerable<ISkillAction> AvailableSkills { get; }

   /// <summary>
   /// Get a skill by its name.
   /// The comparison is case insensitive and culture invariant.
   /// </summary>
   /// <param name="skillName">The name of the skill to get.</param>
   /// <returns>The skill if found, null otherwise.</returns>
   ISkillAction? Get(string skillName);
}

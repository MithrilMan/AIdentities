namespace AIdentities.Shared.Features.CognitiveEngine.Skills;

public interface ISkillManager
{
   /// <summary>
   /// Returns all the known skills.
   /// </summary>
   /// <returns>The list of known skills.</returns>
   IEnumerable<ISkill> All();

   /// <summary>
   /// Get a skill by its name.
   /// The comparison is case insensitive and culture invariant.
   /// </summary>
   /// <param name="skillName">The name of the skill to get.</param>
   /// <returns>The skill if found, null otherwise.</returns>
   ISkill? Get(string skillName);

   /// <summary>
   /// Get a skill by its type.
   /// Useful if we want to get a specific skill programmatically.
   /// </summary>
   /// <typeparam name="TSkill">The type of the skill to get.</typeparam>
   /// <returns>The skill if found, null otherwise.</returns>
   ISkill? Get<TSkill>();
}

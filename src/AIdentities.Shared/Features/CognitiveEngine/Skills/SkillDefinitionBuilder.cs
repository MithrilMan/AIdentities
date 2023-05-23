using AIdentities.Shared.Features.CognitiveEngine.Skills.Attributes;

namespace AIdentities.Shared.Features.CognitiveEngine.Skills;

/// <summary>
/// Class that exhamines a skill, read its skill definition attributes
/// and build a final skill definition.
/// </summary>
public static class SkillDefinitionBuilder
{
   /// <summary>
   /// Build a skill definition from a skill type.
   /// Before building the skill definition, it validates the skill definition attributes.
   /// </summary>
   /// <param name="skillType">The type of the skill.</param>
   /// <returns>The skill definition.</returns>
   /// <exception cref="ArgumentException">If the skill definition attributes are not found or are invalid.</exception>
   public static SkillDefinition BuildSkillDefinition(Type skillType)
   {
      // ensure skillType implements ISkill
      if (!typeof(ISkill).IsAssignableFrom(skillType))
      {
         throw new ArgumentException($"Type {skillType.FullName} does not implement {nameof(ISkill)}");
      }

      SkillDefinitionAttribute skillDefinitionAttribute = GetSkillDefinitionAttribute(skillType);

      var skillDefinition = new SkillDefinition(skillDefinitionAttribute.Name!, skillDefinitionAttribute.Description!)
      {
         Tags = skillDefinitionAttribute.Tags?.ToList() ?? new List<string>()
      };

      List<SkillInputDefinitionAttribute> inputDefinitions = GetSkillInputDefinitions(skillType);
      foreach (var input in inputDefinitions)
      {
         skillDefinition.Inputs.Add(new SkillInputDefinition(
            Name: input.Name!,
            Type: input.Type,
            IsRequired: input.IsRequired,
            Description: input.Description!
            ));
      }

      List<SkillOutputDefinitionAttribute> outputDefinitions = GetSkillOutputDefinitions(skillType);
      foreach (var output in outputDefinitions)
      {
         skillDefinition.Outputs.Add(new SkillOutputDefinition(
            Name: output.Name!,
            Type: output.Type,
            Description: output.Description!
            ));
      }

      List<SkillExampleAttribute> examples = GetSkillExamples(skillType);
      foreach (var example in examples)
      {
         skillDefinition.Examples.Add(example.Example!);
      }

      return skillDefinition;
   }


   /// <summary>
   /// Get the skill definition attribute of a skill and validate it.
   /// </summary>
   /// <param name="skillType">The type of the skill.</param>
   /// <returns>The validated skill definition attribute.</returns>
   /// <exception cref="ArgumentException">If the skill definition attribute is not found or is invalid.</exception>
   private static SkillDefinitionAttribute GetSkillDefinitionAttribute(Type skillType)
   {
      var skillDefinitionAttribute = skillType.GetCustomAttributes(typeof(SkillDefinitionAttribute), false)
         .Cast<SkillDefinitionAttribute>()
         .FirstOrDefault() ?? throw new ArgumentException($"Type {skillType.FullName} does not have a {nameof(SkillDefinitionAttribute)}");

      if (string.IsNullOrWhiteSpace(skillDefinitionAttribute.Name))
      {
         throw new ArgumentException($"Type {skillType.FullName} has an empty {nameof(SkillDefinitionAttribute.Name)}");
      }

      if (string.IsNullOrWhiteSpace(skillDefinitionAttribute.Description))
      {
         throw new ArgumentException($"Type {skillType.FullName} has an empty {nameof(SkillDefinitionAttribute.Description)}");
      }

      return skillDefinitionAttribute;
   }

   /// <summary>
   /// Gets the list of input definitions of a skill and validate them.
   /// </summary>
   /// <param name="skillType">The type of the skill.</param>
   /// <returns>The list of validated input definitions.</returns>
   /// <exception cref="ArgumentException">If the skill input definition attribute is not found or is invalid.</exception>
   private static List<SkillInputDefinitionAttribute> GetSkillInputDefinitions(Type skillType)
   {
      var inputs = skillType.GetCustomAttributes(typeof(SkillInputDefinitionAttribute), false)
         .Cast<SkillInputDefinitionAttribute>()
         .ToList();

      if (inputs.Any(input => string.IsNullOrWhiteSpace(input.Name)))
      {
         throw new ArgumentException($"Type {skillType.FullName} has an empty {nameof(SkillInputDefinitionAttribute.Name)}");
      }

      if (inputs.Any(input => string.IsNullOrWhiteSpace(input.Description)))
      {
         throw new ArgumentException($"Type {skillType.FullName} has an empty {nameof(SkillInputDefinitionAttribute.Description)}");
      }

      if (inputs.Any(input => input.Type == SkillVariableType.Unknown))
      {
         throw new ArgumentException($"Type {skillType.FullName} has an unknown {nameof(SkillInputDefinitionAttribute.Type)}");
      }

      return inputs;
   }

   /// <summary>
   /// Gets the list of output definitions of a skill and validate them.
   /// </summary>
   /// <param name="skillType">The type of the skill.</param>
   /// <returns>The list of validated output definitions.</returns>
   /// <exception cref="ArgumentException">If the skill output definition attribute is not found or is invalid.</exception>
   private static List<SkillOutputDefinitionAttribute> GetSkillOutputDefinitions(Type skillType)
   {
      var outputs = skillType.GetCustomAttributes(typeof(SkillOutputDefinitionAttribute), false)
         .Cast<SkillOutputDefinitionAttribute>()
         .ToList();

      if (outputs.Any(output => string.IsNullOrWhiteSpace(output.Name)))
      {
         throw new ArgumentException($"Type {skillType.FullName} has an empty {nameof(SkillOutputDefinitionAttribute.Name)}");
      }

      if (outputs.Any(output => string.IsNullOrWhiteSpace(output.Description)))
      {
         throw new ArgumentException($"Type {skillType.FullName} has an empty {nameof(SkillOutputDefinitionAttribute.Description)}");
      }

      if (outputs.Any(output => output.Type == SkillVariableType.Unknown))
      {
         throw new ArgumentException($"Type {skillType.FullName} has an unknown {nameof(SkillOutputDefinitionAttribute.Type)}");
      }

      return outputs;
   }

   /// <summary>
   /// Gets the list of examples of a skill and validate them.
   /// </summary>
   /// <param name="skillType">The type of the skill.</param>
   /// <returns>The list of validated examples.</returns>
   /// <exception cref="ArgumentException">If the skill example attribute is not found or is invalid.</exception>
   private static List<SkillExampleAttribute> GetSkillExamples(Type skillType)
   {
      var examples = skillType.GetCustomAttributes(typeof(SkillExampleAttribute), false)
         .Cast<SkillExampleAttribute>()
         .ToList();

      // not sure if Examples shoud be mandatory
      //if (examples.Count == 0)
      //{
      //   throw new ArgumentException($"Type {skillType.FullName} does not have any {nameof(SkillExampleAttribute)}");
      //}

      if (examples.Any(example => string.IsNullOrWhiteSpace(example.Example)))
      {
         throw new ArgumentException($"Type {skillType.FullName} has an empty {nameof(SkillExampleAttribute.Example)}");
      }

      return examples;
   }
}

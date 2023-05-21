namespace AIdentities.Shared.Features.CognitiveEngine.Skills;

public record SkillArgumentDefinition
{
   /// <summary>
   /// The name of the command.
   /// </summary>
   public string Name { get; }

   /// <summary>
   /// The description of the argument.
   /// This is used to instruct the AI when to activate the command.
   /// </summary>
   public string Description { get; }

   /// <summary>
   /// Specifies whether the argument is required.
   /// </summary>
   public bool IsRequired { get; }

   /// <summary>
   /// The type of the argument.
   /// </summary>
   public Type Type { get; }

   // This is a positional argument
   public SkillArgumentDefinition(string name, string description, bool isRequired, Type type = null)
   {
      Name = name;
      Description = description;
      IsRequired = isRequired;
      Type = type;
   }
}

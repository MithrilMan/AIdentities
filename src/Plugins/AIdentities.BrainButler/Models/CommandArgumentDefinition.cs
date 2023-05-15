namespace AIdentities.BrainButler.Models;

public record CommandArgumentDefinition
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

   // This is a positional argument
   public CommandArgumentDefinition(string name, string description, bool isRequired)
   {
      Name = name;
      Description = description;
      IsRequired = isRequired;
   }
}

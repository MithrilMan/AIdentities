namespace AIdentities.Shared.Features.CognitiveEngine.Skills.Attributes;

/// <summary>
/// An attribute used to create an example of how to use the skill.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class SkillExampleAttribute : Attribute
{
   /// <summary>
   /// A full example of how to use the skill.
   /// This must comprehend a fragment of a full exchange between the initial prompt and the final prompt
   /// that will generate eventually the structure containing the arguments extracted by the prompt.
   /// The important thing to remember is that it must be coherent with the structure of the skill.
   /// </summary>
   /// <example>
   /// Assuming a skill has an argument named "WhoToInvite" of type string, the skill is called "InviteSomeone"
   /// and the CognitiveEngine uses the LLM to generate a valid JSON to parse and get its variable from there,
   /// a valid example could be:
   /// 
   /// UserRequest: I'd like to talk with MithrilMan
   /// Reasoning: The user wants to talk with MithrilMan.
   /// JSON: { "WhoToInvite": "MithrilMan" }
   /// </example>
   public string Example { get; init; } = default!;
}

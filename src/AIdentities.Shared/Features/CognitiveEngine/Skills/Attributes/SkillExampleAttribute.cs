namespace AIdentities.Shared.Features.CognitiveEngine.Skills.Attributes;

/// <summary>
/// An attribute used to create an example of how to use the skill.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class SkillExampleAttribute : Attribute
{
   /// <summary>
   /// A full example of how to use the skill.
   /// 
   /// By default the skill are expected to request a JSON formatted response this is why you can
   /// fill explicit fields in this attribute like <see cref="UserRequest"/>, <see cref="Reasoning"/> and
   /// <see cref="JsonExample"/> that will work better with the default prompt engine.
   /// 
   /// However if you want have more control over the prompt you can use this field and you may want to
   /// have a custom CognitiveEngine that uses a custom prompt engine to make use of the example properly.
   /// 
   /// </summary>
   /// <example>
   /// Assuming a skill has an argument named "WhoToInvite" of type string, the skill is called "InviteSomeone"
   /// and the CognitiveEngine uses the LLM to generate a valid JSON to parse and get its variable from there,
   /// a valid example for the default prompt engine could be:
   /// 
   /// UserRequest: I'd like to talk with MithrilMan
   /// Reasoning: The user wants to talk with MithrilMan.
   /// JSON: { "WhoToInvite": "MithrilMan" }
   /// </example>
   public string? CustomExample { get; init; }


   /// <summary>
   /// The user request portion of the example.
   /// </summary>
   public string? UserRequest { get; init; }

   /// <summary>
   /// The reasoning portion of the example.
   /// </summary>
   public string? Reasoning { get; init; }

   /// <summary>
   /// The JSON portion of the example.
   /// </summary>
   public string? JsonExample { get; init; }
}

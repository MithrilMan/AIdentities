using AIdentities.Shared.Features.CognitiveEngine.Thoughts;

namespace AIdentities.Shared.Features.CognitiveEngine;

public class CognitiveContext
{
   public AIdentity AIdentity { get; }
   public Dictionary<string, object?> State { get; set; } = new();

   public CognitiveContext(AIdentity aIdentity)
   {
      AIdentity = aIdentity;
   }

   /// <summary>
   /// Generates an <see cref="InvalidPromptResponseThought"/>.
   /// This is a special type of <see cref="IntrospectiveThought"/> that is used when the
   /// cognitive engine is not able to handle the conversion the AIdentity response to a prompt
   /// into a valid value for the skill action.
   /// </summary>
   /// <param name="skillAction">The optional skill action that is being executed.</param>
   /// <returns>The invalid prompt thought.</returns>
   public InvalidPromptThought InvalidPrompt(ISkillAction? skillAction)
      => new(skillAction?.Id, AIdentity);

   /// <summary>
   /// Generates an <see cref="InvalidPromptResponseThought"/>.
   /// This is a special type of <see cref="IntrospectiveThought"/> that is used when the 
   /// cognitive engine is not able to understand the user's response to a prompt.
   /// In this specific case, the caller should try to provide missing arguments before invoking again the action.
   /// </summary>
   /// <param name="skillAction">The optional skill action that is being executed.</param>
   /// <param name="missingArguments">The list of missing arguments.</param>
   /// <returns>The invalid prompt response thought.</returns>
   public MissingArgumentsThought MissingArguments(ISkillAction? skillAction, params SkillArgumentDefinition[] missingArguments)
      => new MissingArgumentsThought(skillAction?.Id, AIdentity, missingArguments);

   /// <summary>
   /// Produces an <see cref="ActionThought"/>.
   /// Consider this as a way to log what the cognitive engine is doing.
   /// </summary>
   /// <param name="skillAction">The optional skill action that is being executed.</param>
   /// <param name="thought">The thought to produce.</param>
   /// <returns>The action thought.</returns>
   public ActionThought ActionThought(ISkillAction? skillAction, string thought)
      => new ActionThought(skillAction?.Id, AIdentity, thought);

   /// <summary>
   /// Generates a <see cref="FinalThought"/>.
   /// A final thought is something that the cognitive engine can return to the user.
   /// </summary>
   /// <param name="skillAction">The optional skill action that is being executed.</param>
   /// <param name="thought">The thought to produce.</param>
   /// <returns>The final thought.</returns>
   public FinalThought FinalThought(ISkillAction? skillAction, string thought)
      => new FinalThought(skillAction?.Id, AIdentity.Id, thought);

   /// <summary>
   /// Generates a <see cref="StreamedThought"/>.
   /// A streamed thought is supposed to be updated until it generates the last chunk of information.
   /// At that point StreamedThought.IsStreamComplete should be set to true.
   /// </summary>
   /// <param name="skillAction">The optional skill action that is being executed.</param>
   /// <param name="thought">The thought to produce.</param>
   /// <returns>The first streamed thought.</returns>
   public StreamedFinalThought StreamFinalThought(ISkillAction? skillAction, string thought)
      => new StreamedFinalThought(skillAction?.Id, AIdentity.Id, thought);

   /// <summary>
   /// Generates a <see cref="StreamedThought"/>.
   /// A streamed thought is supposed to be updated until it generates the last chunk of information.
   /// At that point StreamedThought.IsStreamComplete should be set to true.
   /// </summary>
   /// <param name="skillAction">The optional skill action that is being executed.</param>
   /// <param name="thought">The thought to produce.</param>
   /// <returns>The first streamed thought.</returns>
   public StreamedIntrospectiveThought StreamIntrospectiveThought(ISkillAction? skillAction, string thought)
      => new StreamedIntrospectiveThought(skillAction?.Id, AIdentity, thought);
}

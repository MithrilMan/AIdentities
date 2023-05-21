using AIdentities.Shared.Features.CognitiveEngine.Thoughts;

namespace AIdentities.Shared.Features.CognitiveEngine;

public class CognitiveContext
{
   public AIdentity AIdentity { get; }
   public Dictionary<string, object?> StateObjects { get; set; } = new();

   public CognitiveContext(AIdentity aIdentity)
   {
      AIdentity = aIdentity;
   }

   /// <summary>
   /// Generates an <see cref="InvalidPromptResponseThought"/> for the given.
   /// </summary>
   /// <returns></returns>
   public InvalidPromptThought InvalidPrompt(ISkillAction? skillAction)
      => new(skillAction?.Id, AIdentity);

   public MissingArgumentsThought MissingArguments(ISkillAction? skillAction, params SkillArgumentDefinition[] missingArguments)
      => new MissingArgumentsThought(skillAction?.Id, AIdentity, missingArguments);

   public Thought Thinking(ISkillAction? skillAction, string thought, bool isFinal = false)
      => new Thought(skillAction?.Id, AIdentity.Id, thought)
      {
         IsFinal = isFinal,
         Content = thought,
      };

   public FinalThought FinalThought(ISkillAction? skillAction, string thought)
      => new FinalThought(skillAction?.Id, AIdentity.Id, thought);
}

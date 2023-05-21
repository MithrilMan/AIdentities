using System.Diagnostics.CodeAnalysis;
using AIdentities.Shared.Features.CognitiveEngine.Mission;
using AIdentities.Shared.Features.CognitiveEngine.Thoughts;

namespace AIdentities.Shared.Features.CognitiveEngine.Skills;

public abstract class SkillDefinition : ISkillAction
{
   public Guid Id { get; } = Guid.NewGuid();
   public string Name { get; }
   public string ActivationContext { get; }
   public IEnumerable<SkillArgumentDefinition> Arguments { get; protected set; } = Enumerable.Empty<SkillArgumentDefinition>();
   public string ReturnDescription { get; }
   public string Examples { get; }

   public SkillDefinition(string name,
                          string activationContext,
                          string returnDescription,
                          string examples)
   {
      Name = name;
      ActivationContext = activationContext;
      ReturnDescription = returnDescription;
      Examples = examples;
   }

   public abstract IAsyncEnumerable<Thought> ExecuteAsync(Prompt prompt, CognitiveContext cognitiveContext, MissionContext missionContext, CancellationToken cancellationToken);

   /// <summary>
   /// Extracts the typeod arguments from the text.
   /// </summary>
   /// <typeparam name="TReturnValue">The type of the arguments to extract.</typeparam>
   /// <param name="text">The text to extract the arguments from.</param>
   /// <returns>The extracted arguments.</returns>
   public virtual bool TryExtractJson<TReturnValue>(string text, [MaybeNullWhen(false)] out TReturnValue args) where TReturnValue : class
   {
      var json = SkillRegexUtils.ExtractJson().Match(text).Value;
      try
      {
         args = JsonSerializer.Deserialize<TReturnValue>(json)!;
         return true;
      }
      catch (Exception)
      {
         args = default;
         return false;
      }
   }

   public virtual bool TryExtractJson(string text, [MaybeNullWhen(false)] out string json)
   {
      json = SkillRegexUtils.ExtractJson().Match(text).Value;
      return !string.IsNullOrWhiteSpace(json);
   }


}

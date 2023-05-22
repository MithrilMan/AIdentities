using AIdentities.Shared.Features.CognitiveEngine.Thoughts;

namespace AIdentities.Shared.Features.CognitiveEngine;

public class CognitiveContext
{
   public AIdentity AIdentity { get; }
   public Dictionary<string, object?> State { get; set; } = new();

   /// <summary>
   /// A common way to store json arguments for a skill in a specific cognitive context.
   /// </summary>
   /// <param name="skillId"></param>
   /// <returns>The json formatted arguments.</returns>
   public string? GetSkillJsonArgs(Guid skillId)
   {
      return GetOrDefault<string>($"SkillJsonArgs_{skillId}");
   }

   /// <summary>
   /// A common way to store json arguments for a skill in a specific cognitive context.
   /// </summary>
   /// <param name="skillId">The skill id.</param>
   /// <param name="value">The json formatted arguments.</param>
   public void SetSkillJsonArgs(Guid skillId, string? value)
   {
      SetOrRemove($"SkillJsonArgs_{skillId}", value);
   }


   /// <summary>
   /// Get the value of the given key in the State dictionary.
   /// If the key is not present or the value is not of the given type, return the default value (cannot be null).
   /// </summary>
   /// <typeparam name="T">The type of the value to get.</typeparam>
   /// <param name="key">The key of the value to get.</param>
   /// <param name="defaultValue">The default value to return if the key is not present or the value is not of the given type.
   /// The default value cannot be null and will be stored in the dictionary.
   /// </param>
   /// <returns>The value of the given key in the State dictionary or the default (non null) value.</returns>
   /// <exception cref="ArgumentNullException"></exception>
   protected T GetOrDefault<T>(string key, T defaultValue)
   {
      if (defaultValue is null) throw new ArgumentNullException(nameof(defaultValue));

      if (!State.TryGetValue(key, out var obj) || obj is not T value)
      {
         value = defaultValue;
         State[key] = value!;
      }

      return value;
   }

   /// <summary>
   /// Get the value of the given key in the State dictionary.
   /// If the key is not present or the value is not of the given type, return null.
   /// </summary>
   /// <typeparam name="T">The type of the value to get.</typeparam>
   /// <param name="key">The key of the value to get.</param>
   /// <returns>The value of the given key in the State dictionary or the default value of the type.</returns>
   protected T? GetOrDefault<T>(string key)
   {
      if (!State.TryGetValue(key, out var obj) || obj is not T value)
      {
         return default;
      }

      return value;
   }

   /// <summary>
   /// Set the value of the given key in the State dictionary.
   /// if the passed value is null, the key is removed from the dictionary.
   /// </summary>
   /// <typeparam name="T">The type of the value to set.</typeparam>
   /// <param name="key">The key of the value to set.</param>
   /// <param name="value">The value to set.</param>
   protected void SetOrRemove<T>(string key, T? value)
   {
      if (value is null)
      {
         State.Remove(key);
      }
      else
      {
         State[key] = value;
      }
   }

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
   public InvalidPromptThought InvalidPrompt(ISkill? skillAction)
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
   public MissingArgumentsThought MissingArguments(ISkill? skillAction, params string[] missingArguments)
      => new MissingArgumentsThought(skillAction?.Id, AIdentity, missingArguments);

   public IntrospectiveThought IntrospectiveThought(ISkill? skillAction, string thought)
      => new IntrospectiveThought(skillAction?.Id, AIdentity, thought);

   /// <summary>
   /// Produces an <see cref="ActionThought"/>.
   /// Consider this as a way to log what the cognitive engine is doing.
   /// </summary>
   /// <param name="skillAction">The optional skill action that is being executed.</param>
   /// <param name="thought">The thought to produce.</param>
   /// <returns>The action thought.</returns>
   public ActionThought ActionThought(ISkill? skillAction, string thought)
      => new ActionThought(skillAction?.Id, AIdentity, thought);

   /// <summary>
   /// Generates a <see cref="FinalThought"/>.
   /// A final thought is something that the cognitive engine can return to the user.
   /// </summary>
   /// <param name="skillAction">The optional skill action that is being executed.</param>
   /// <param name="thought">The thought to produce.</param>
   /// <returns>The final thought.</returns>
   public FinalThought FinalThought(ISkill? skillAction, string thought)
      => new FinalThought(skillAction?.Id, AIdentity.Id, thought);

   /// <summary>
   /// Generates a <see cref="StreamedThought"/>.
   /// A streamed thought is supposed to be updated until it generates the last chunk of information.
   /// At that point StreamedThought.IsStreamComplete should be set to true.
   /// </summary>
   /// <param name="skillAction">The optional skill action that is being executed.</param>
   /// <param name="thought">The thought to produce.</param>
   /// <returns>The first streamed thought.</returns>
   public StreamedFinalThought StreamFinalThought(ISkill? skillAction, string thought)
      => new StreamedFinalThought(skillAction?.Id, AIdentity.Id, thought);

   /// <summary>
   /// Generates a <see cref="StreamedThought"/>.
   /// A streamed thought is supposed to be updated until it generates the last chunk of information.
   /// At that point StreamedThought.IsStreamComplete should be set to true.
   /// </summary>
   /// <param name="skillAction">The optional skill action that is being executed.</param>
   /// <param name="thought">The thought to produce.</param>
   /// <returns>The first streamed thought.</returns>
   public StreamedIntrospectiveThought StreamIntrospectiveThought(ISkill? skillAction, string thought)
      => new StreamedIntrospectiveThought(skillAction?.Id, AIdentity, thought);
}

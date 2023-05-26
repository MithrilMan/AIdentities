namespace AIdentities.Shared.Features.CognitiveEngine.Thoughts;

public interface IThought
{
   /// <summary>
   /// An unique identifier for the thought.
   /// Especially useful to manage streamed thoughts.
   /// </summary>
   Guid ThoughtId { get; }

   /// <summary>
   /// The skill action that created the thought.
   /// </summary>
   string? SkillName { get; set; }

   /// <summary>
   /// The AIdentity that created the thought.
   /// </summary>
   Guid AIdentityId { get; set; }

   /// <summary>
   /// The content of the thought.
   /// A final thought could be considered as the output toward the user.
   /// </summary>
   string Content { get; set; }
}

namespace AIdentities.Shared.Features.CognitiveEngine.Thoughts;

public record Thought(Guid? SkillActionId, Guid AIdentityId, string Content)
{
   /// <summary>
   /// The skill action that created the thought.
   /// </summary>
   public Guid? SkillActionId { get; set; } = SkillActionId;

   /// <summary>
   /// The AIdentity that created the thought.
   /// </summary>
   public Guid AIdentityId { get; set; } = AIdentityId;

   /// <summary>
   /// when the thought is final, it could be sent to the user.
   /// </summary>
   public bool IsFinal { get; set; }

   /// <summary>
   /// The content of the thought.
   /// A final thought could be considered as the output toward the user.
   /// </summary>
   public string Content { get; set; } = Content;
}

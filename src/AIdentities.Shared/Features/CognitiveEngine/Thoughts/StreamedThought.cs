namespace AIdentities.Shared.Features.CognitiveEngine.Thoughts;

public record StreamedThought(Guid? SkillActionId, Guid AIdentityId, string Content, bool IsLastStreamPiece) : Thought(SkillActionId, AIdentityId, Content)
{
   /// <summary>
   /// When the thought is streamed, it could be sent to the user as soon as it's created.
   /// </summary>
   public override bool IsStreamed => true;

   /// <summary>
   /// Specifies if the current streamed thought is the last piece of the stream.
   /// </summary>
   public bool IsLastStreamPiece { get; set; } = IsLastStreamPiece;
}

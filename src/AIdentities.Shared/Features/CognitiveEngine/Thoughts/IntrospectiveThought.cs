namespace AIdentities.Shared.Features.CognitiveEngine.Thoughts;

/// <summary>
/// An introspective thought is a thought that is created when the cognitive engine doesn't have enough data to 
/// process the request and have therefore to do something in order to proceed.
/// An introspective thought can be handled by the cognitive engine itself or by the user/ AIdentities and can generate
/// other actions to execute, or even sub-missions.
/// </summary>
public record IntrospectiveThought : Thought
{
   public IntrospectiveThought(Guid? SkillActionId, AIdentity AIdentity, string Content)
      : base(SkillActionId, AIdentity.Id, Content)
   {
   }
}

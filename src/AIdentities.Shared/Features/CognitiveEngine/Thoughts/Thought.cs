﻿namespace AIdentities.Shared.Features.CognitiveEngine.Thoughts;

/// <summary>
/// A thought is something that the cognitive engine can produce.
/// It can be a final thought, that is something that the cognitive engine can return to the user,
/// or an introspective thought, that is something that the cognitive engine can use to do something else.
/// A thought can be streamed, that is something that the cognitive engine can return to the user as soon as it's created.
/// Each thought has an unique identifier that can be used to manage streamed thoughts.
/// Each thought has a skill action identifier that can be used to manage the skill action that created the thought.
/// </summary>
/// <param name="skillName">The optional skill name that created the thought.</param>
/// <param name="AIdentityId">The AIdentity that created the thought.</param>
/// <param name="Content">The content of the thought.</param>
public abstract record Thought(string? SkillName, Guid AIdentityId, string Content) : IThought
{
   /// <summary>
   /// The skill action that created the thought.
   /// </summary>
   public string? SkillName { get; set; } = SkillName;

   /// <summary>
   /// The AIdentity that created the thought.
   /// </summary>
   public Guid AIdentityId { get; set; } = AIdentityId;

   /// <summary>
   /// The content of the thought.
   /// A final thought could be considered as the output toward the user.
   /// </summary>
   public string Content { get; set; } = Content;
}

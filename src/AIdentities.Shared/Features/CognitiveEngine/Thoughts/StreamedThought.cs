namespace AIdentities.Shared.Features.CognitiveEngine.Thoughts;

/// <summary>
/// A streamed thought is a thought that isn't returned as a whole, but it's streamed to the user.
/// To improve the dev experience, the streamed thought instance is preserved every time a new chunk is appended,
/// just its text is updated.
/// This way the consumer doesn't have to build the final thought by itself but can just look at the flag
/// IsStreamComplete to know if the thought is complete.
/// </summary>
/// <param name="SkillActionId"></param>
/// <param name="AIdentity"></param>
/// <param name="Content"></param>
/// <param name="IsStreamComplete"></param>
public abstract record StreamedThought : Thought
{
   /// <summary>
   /// Specifies if the current streamed thought is completed.
   /// </summary>
   public bool IsStreamComplete { get; set; }

   public StreamedThought(string? skillName, AIdentity aIdentity, string content, bool isStreamComplete)
      : base(skillName, aIdentity.Id, content)
   {
      IsStreamComplete = isStreamComplete;
   }

   /// <summary>
   /// Append content to the thought.
   /// </summary>
   /// <param name="content"></param>
   public void AppendContent(string content)
   {
      if (IsStreamComplete) throw new InvalidOperationException("Cannot append content to a completed streamed thought.");

      Content ??= "";
      Content += content;
   }


   /// <summary>
   /// Mark the stramed thought as completed.
   /// Subsequent calls to AppendContent will throw an exception.
   /// </summary>
   /// <returns></returns>
   public StreamedThought Completed()
   {
      IsStreamComplete = true;
      return this;
   }
}

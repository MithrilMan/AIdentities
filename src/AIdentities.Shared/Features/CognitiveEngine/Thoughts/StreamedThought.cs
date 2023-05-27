namespace AIdentities.Shared.Features.CognitiveEngine.Thoughts;

/// <summary>
/// A streamed thought is a thought that isn't returned as a whole, but it's streamed to the user.
/// To improve the dev experience, the streamed thought instance is preserved every time a new chunk is appended,
/// just its text is updated.
/// This way the consumer doesn't have to build the final thought by itself but can just look at the flag
/// IsStreamComplete to know if the thought is complete.
/// The Id property is used to identify the same thought chunk in a streamed thought so the consumer knows if
/// an incoming thought chunk belongs to a new thought or is a continuation of the previous one.
/// </summary>
public abstract record StreamedThought : Thought, IStreamedThought
{
   /// <inheritdoc />
   public Guid Id { get; init; } = Guid.NewGuid();

   /// <inheritdoc />
   public bool IsStreamComplete { get; private set; }

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
   /// Return a new StreamThought Mark the stramed thought as completed.
   /// Subsequent calls to AppendContent will throw an exception.
   /// </summary>
   /// <returns></returns>
   public StreamedThought Completed()
   {

      return this with
      {
         IsStreamComplete = true
      };
   }
}

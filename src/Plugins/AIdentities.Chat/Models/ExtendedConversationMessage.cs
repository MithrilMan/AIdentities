namespace AIdentities.Chat.Models;
/// <summary>
/// Extended version of <see cref="ConversationMessage"/> and allow to track custom states like
/// if the audio is being generated or if the message has been fully processed.
/// </summary>
public record ExtendedConversationMessage : ConversationMessage
{
   /// <summary>
   /// True if a a speech is being generated.
   /// </summary>
   public bool IsGeneratingSpeech { get; set; }

   /// <summary>
   /// True if the message is complete.
   /// A streamed thought may generate messages in multiple steps, and this flag is used to track the state.
   /// </summary>
   public bool IsComplete { get; set; }

   public ExtendedConversationMessage(string text, Guid humanId, string humanName, bool isGeneratingSpeech, bool isComplete)
      : base(text, humanId, humanName)
   {
      IsGeneratingSpeech = isGeneratingSpeech;
      IsComplete = isComplete;
   }

   public ExtendedConversationMessage(string text, AIdentity aIdentity, bool isGeneratingSpeech, bool isComplete)
      : base(text, aIdentity)
   {
      IsGeneratingSpeech = isGeneratingSpeech;
      IsComplete = isComplete;
   }
}

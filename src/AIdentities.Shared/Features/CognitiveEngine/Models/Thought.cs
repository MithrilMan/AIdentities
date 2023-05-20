namespace AIdentities.Shared.Features.CognitiveEngine.Models;

public record Thought
{
   /// <summary>
   /// when the thought is final, it could be sent to the user.
   /// </summary>
   public bool IsFinal { get; set; }

   /// <summary>
   /// The content of the thought.
   /// A final thought could be considered as the output toward the user.
   /// </summary>
   public string Content { get; set; } = string.Empty;
}

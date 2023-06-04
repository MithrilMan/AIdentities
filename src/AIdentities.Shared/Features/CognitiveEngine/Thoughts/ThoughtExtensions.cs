using System.Diagnostics.CodeAnalysis;

namespace AIdentities.Shared.Features.CognitiveEngine.Thoughts;

/// <summary>
/// An extension class for <see cref="Thought"/> to add some useful methods to
/// perform some checks on the thought type.
/// </summary>
public static class ThoughtExtensions
{
   /// <summary>
   /// Returns true if the thought is a final thought.
   /// </summary>
   /// <param name="thought">The thought to check.</param>
   /// <returns>True if the thought is a final thought.</returns>
   public static bool IsFinalThought(this Thought thought) => thought is IFinalThought;

   /// <summary>
   /// Returns true if the thought is a streamed thought.
   /// </summary>
   /// <param name="thought">The thought to check.</param>
   /// <returns>True if the thought is a streamed thought.</returns>
   public static bool IsStreamedThought(this Thought thought) => thought is IStreamedThought;

   /// <summary>
   /// Returns true if the thought is a streamed final thought.
   /// </summary>
   /// <param name="thought">The thought to check.</param>
   /// <returns>True if the thought is a streamed final thought.</returns>
   public static bool IsIntrospectiveThought(this Thought thought) => thought is IIntrospectiveThought;

   /// <summary>
   /// Returns true if the thought is a final thought.
   /// </summary>
   /// <param name="thought">The thought to check.</param>
   /// <param name="finalThought">The final thought if it is of type IFinalThought.</param>
   /// <returns>True if the thought is a final thought.</returns>
   public static bool IsFinalThought(this Thought thought, [MaybeNullWhen(false)]out IFinalThought finalThought)
   {
      finalThought = thought as IFinalThought;
      return finalThought != null;
   }

   /// <summary>
   /// Returns true if the thought is a streamed thought.
   /// </summary>
   /// <param name="thought">The thought to check.</param>
   /// <param name="streamedThought">The streamed thought if it is of type IStreamedThought.</param>
   /// <returns>True if the thought is a streamed thought.</returns>
   public static bool IsStreamedThought(this Thought thought, [MaybeNullWhen(false)] out IStreamedThought streamedThought)
   {
      streamedThought = thought as IStreamedThought;
      return streamedThought != null;
   }

   /// <summary>
   /// Returns true if the thought is an introspective thought.
   /// </summary>
   /// <param name="thought">The thought to check.</param>
   /// <param name="introspectiveThought">The introspective thought if it is of type IIntrospectiveThought.</param>
   /// <returns>True if the thought is an introspective thought.</returns>
   public static bool IsIntrospectiveThought(this Thought thought, [MaybeNullWhen(false)] out IIntrospectiveThought introspectiveThought)
   {
      introspectiveThought = thought as IIntrospectiveThought;
      return introspectiveThought != null;
   }
}

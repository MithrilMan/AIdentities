﻿namespace AIdentities.Shared.Features.CognitiveEngine.Thoughts;

/// <summary>
/// A streamed thought is a thought that isn't returned as a whole, but it's streamed to the user.
/// When IsStreamComplete is true, the thought is completed and will not be updated anymore.
/// To not have to check both streamed and non-streamed results, we use this interface to mark the streamed thoughts.
/// </summary>
public interface IStreamedThought : IThought
{
   /// <summary>
   /// The thought Id.
   /// Useful to identify the same thought chunk in a streamed thought.
   /// </summary>
   Guid Id { get; init; }

   /// <summary>
   /// Specifies if the current streamed thought is completed.
   /// </summary>
   bool IsStreamComplete { get; }
}

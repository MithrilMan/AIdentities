using System.Diagnostics.CodeAnalysis;
using AIdentities.Shared.Features.CognitiveEngine.Prompts;
using AIdentities.Shared.Features.CognitiveEngine.Thoughts;

namespace AIdentities.Shared.Features.CognitiveEngine.Mission;

/// <summary>
/// What the user wants to achieve.
/// A mission can have a set of constraints that affect the way the cognitive engine will work.
/// </summary>
public class Mission<TMissionContext> : IMission, IDisposable
   where TMissionContext : IMissionContext, new()
{
   /// <inheritdoc/>
   public string Goal { get; set; } = "";

   /// <summary>
   /// The specialized mission context.
   /// </summary>
   public TMissionContext Context { get; set; } = new();

   /// <inheritdoc/>
   public CancellationToken MissionRunningCancellationToken { get; private set; }

   /// <inheritdoc/>
   IMissionContext IMission.Context { get => Context; }

   /// <inheritdoc/>
   public ICognitiveEngine? MissionRunner { get; private set; }

   [MemberNotNullWhen(true, nameof(MissionRunner))]
   /// <inheritdoc/>
   public bool IsRunning { get; private set; }

   /// <summary>
   /// Keep track if the mission has run at least once.
   /// If the mission has run at least once, the mission cannot be started again but a new instance of the mission should be created.
   /// </summary>
   private bool _hasRunOnce { get; set; } = false;

   /// <inheritdoc/>
   public CancellationTokenSource? MissionCancellationTokenSource { get; private set; }

   readonly object _lock = new();

   /// <inheritdoc/>
   public virtual void Start(ICognitiveEngine cognitiveEngine, CancellationToken missionCancellationToken)
   {
      if (IsRunning) throw new InvalidOperationException("The mission is already running.");
      if (_hasRunOnce) throw new InvalidOperationException("The mission has already run once. Create a new instance of the mission.");

      lock (_lock)
      {
         IsRunning = true;
         _hasRunOnce = true;
         MissionRunner = cognitiveEngine;
         MissionCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(missionCancellationToken);
         Context.MissionRunningCancellationToken = MissionCancellationTokenSource.Token;
         Context.Goal = Goal;
      }
   }

   /// <inheritdoc/>
   public virtual void Stop()
   {
      lock (_lock)
      {
         IsRunning = false;
         MissionCancellationTokenSource?.Cancel();
         MissionCancellationTokenSource?.Dispose();
         MissionCancellationTokenSource = null;
      }
   }

   public virtual void Dispose()
   {
      try
      {
         MissionCancellationTokenSource?.Dispose();
      }
      finally { }

      MissionCancellationTokenSource = null;
   }

   /// <summary>
   /// Talk to the mission runner.
   /// Interacting with the mission runner is a way to control or modify the mission.
   /// </summary>
   /// <param name="prompt">The prompt that the mission runner has to handle.</param>
   /// <param name="cancellationToken">The operation cancellation token.</param>
   public virtual async IAsyncEnumerable<Thought> TalkToMissionRunnerAsync(Prompt prompt, [EnumeratorCancellation] CancellationToken cancellationToken = default)
   {
      if (!IsRunning) throw new InvalidOperationException("The mission is not running.");

      var thoughts = MissionRunner.HandlePromptAsync(prompt, Context, cancellationToken).ConfigureAwait(false);

      await foreach (var thought in thoughts)
      {
         yield return thought;
      }
   }
}

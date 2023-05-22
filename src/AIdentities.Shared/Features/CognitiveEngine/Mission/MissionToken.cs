namespace AIdentities.Shared.Features.CognitiveEngine.Mission;

/// <summary>
/// This token is used to cancel a mission execution.
/// Technically it makes use of a CancellationTokenSource that's linked to the CancellationToken
/// used to start the mission with <see cref="ICognitiveEngine.StartMissionAsync"/>.
/// <see cref="StopMission"/> can be called only once and cannot be reused.
/// This token has to be disposed to release the resources.
/// Note that once this token is disposed, it will stop the mission if it's still running.
/// </summary>
public class MissionToken : IDisposable
{
   private CancellationTokenSource? _cancellationTokenSource;

   public IMission Mission { get; }

   internal CancellationToken CancellationToken => _cancellationTokenSource?.Token ?? CancellationToken.None;

   public MissionToken(IMission mission, params CancellationToken[] linkedCancellationTokens)
   {
      Mission = mission;
      _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(linkedCancellationTokens);
   }

   public void StopMission()
   {
      if (_cancellationTokenSource is null)
      {
         throw new InvalidOperationException("The mission has already been cancelled.");
      }
      _cancellationTokenSource.Cancel();
      _cancellationTokenSource = null;
   }

   public void Dispose()
   {
      try
      {
         _cancellationTokenSource?.Dispose();
      }
      finally { }

      _cancellationTokenSource = null;
   }
}

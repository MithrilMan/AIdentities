using AIdentities.Shared.Features.CognitiveEngine.Models;

namespace AIdentities.Shared.Features.CognitiveEngine;

interface IMissionManager
{
   /// <summary>
   /// Starts a mission.
   /// </summary>
   /// <param name="mission">The mission to start.</param>
   /// <param name="cancellationToken">The cancellation token that can be used to stop the mission execution.</param>
   /// <returns></returns>
   public Task StartMissionAsync(Mission mission, AIdentity missionLeader, CancellationToken cancellationToken);
}

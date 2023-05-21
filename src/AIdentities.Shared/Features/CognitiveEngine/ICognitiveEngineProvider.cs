namespace AIdentities.Shared.Features.CognitiveEngine;

public interface ICognitiveEngineProvider
{
   ICognitiveEngine CreateCognitiveEngine<TCognitiveEngine>(AIdentity aIdentity)
      where TCognitiveEngine : ICognitiveEngine;
}

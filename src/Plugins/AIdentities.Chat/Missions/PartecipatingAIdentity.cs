namespace AIdentities.Chat.Missions;

/// <summary>
/// holds a reference to the AIdentity engine.
/// </summary>
public class ParticipatingAIdentity
{
   public ICognitiveEngine CognitiveEngine { get; set; }
   public AIdentity AIdentity => CognitiveEngine.AIdentity;

   public ParticipatingAIdentity(ICognitiveEngine engine)
   {
      CognitiveEngine = engine;
   }
}

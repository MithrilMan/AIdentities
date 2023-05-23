namespace AIdentities.Chat.Missions;

/// <summary>
/// holds a reference to the AIdentity engine.
/// </summary>
public class PartecipatingAIdentity
{
   public ICognitiveEngine CognitiveEngine { get; set; }
   public AIdentity AIdentity => CognitiveEngine.AIdentity;

   public PartecipatingAIdentity(ICognitiveEngine engine)
   {
      CognitiveEngine = engine;
   }
}

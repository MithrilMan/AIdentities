namespace AIdentities.BrainButler.Services.CoT;

public class ChainOfThought
{
   readonly IConnectorsManager _connectorsManager;
   private readonly List<Thought> _thoughts = new List<Thought>();

   public ChainOfThought(IConnectorsManager connectorsManager)
   {
      _connectorsManager = connectorsManager;
   }

   public void AddThought(string content)
   {
      _thoughts.Add(new Thought(content));
   }

   public async Task SendToLLM(CancellationToken cancellationToken)
   {
      var connector = _connectorsManager.GetCompletionConnector()
         ?? throw new Exception("No completion connector found");

      foreach (var thought in _thoughts)
      {
         await thought.SendToLLM(connector, cancellationToken).ConfigureAwait(false);
      }
   }
}

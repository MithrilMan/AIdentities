namespace AIdentities.BrainButler.Models;

public abstract class CommandDefinition : IBrainButlerCommand
{
   public string Name { get; }
   public string ActivationContext { get; }
   public IEnumerable<CommandArgumentDefinition> Arguments { get; protected set; } = Enumerable.Empty<CommandArgumentDefinition>();
   public string ReturnDescription { get; }
   public string Examples { get; }

   public CommandDefinition(string name,
                            string activationContext,
                            string returnDescription,
                            string examples)
   {
      Name = name;
      ActivationContext = activationContext;
      ReturnDescription = returnDescription;
      Examples = examples;
   }

   public abstract IAsyncEnumerable<CommandExecutionStreamedFragment> ExecuteAsync(string userPrompt, string? inputPrompt, CancellationToken cancellationToken = default);
}

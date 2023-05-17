namespace AIdentities.BrainButler.Services;

public interface IBrainButlerCommandManager
{
   /// <summary>
   /// The list of available commands.
   /// </summary>
   IEnumerable<IBrainButlerCommand> AvailableCommands { get; }
}

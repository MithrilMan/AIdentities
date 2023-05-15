using AIdentities.BrainButler.Models;

namespace AIdentities.BrainButler.Services;

public class BrainButlerCommandManager : IBrainButlerCommandManager
{
   readonly ILogger<BrainButlerCommandManager> _logger;

   public IEnumerable<IBrainButlerCommand> AvailableCommands { get; }

   public BrainButlerCommandManager(ILogger<BrainButlerCommandManager> logger, IEnumerable<IBrainButlerCommand> availableCommands)
   {
      _logger = logger;
      AvailableCommands = availableCommands;
   }


}

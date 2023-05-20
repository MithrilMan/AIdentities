using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIdentities.Shared.Features.CognitiveEngine.Models;
using AIdentities.Shared.Plugins.Connectors;
using AIdentities.Shared.Plugins.Connectors.Completion;

namespace AIdentities.Shared.Features.CognitiveEngine.Engines;
public class MithrilCognitiveEngine : CognitiveEngine
{
   readonly ILogger<MithrilCognitiveEngine> _logger;

   //private Queue<>

   public MithrilCognitiveEngine(ILogger<MithrilCognitiveEngine> logger, ICompletionConnector completionConnector, AIdentity aIdentity) : base(aIdentity)
   {

   }

   public override IAsyncEnumerable<Thought> HandlePrompt(Prompt prompt) => prompt switch
   {
      UserPrompt userPrompt => HandleUserPrompt(userPrompt),
      SKillResultPrompt skillResultPrompt => HandleSKillResultPrompt(skillResultPrompt),
      ThoughtResultPrompt thoughtResultPrompt => HandleThoughtResultPrompt(thoughtResultPrompt),
      _ => throw new NotImplementedException(),
   };

   protected IAsyncEnumerable<Thought> HandleUserPrompt(UserPrompt prompt)
   {
      throw new NotImplementedException();
   }

   protected IAsyncEnumerable<Thought> HandleThoughtResultPrompt(ThoughtResultPrompt thoughtResultPrompt)
   {
      throw new NotImplementedException();
   }

   protected IAsyncEnumerable<Thought> HandleSKillResultPrompt(SKillResultPrompt skillResultPrompt)
   {
      throw new NotImplementedException();
   }

}

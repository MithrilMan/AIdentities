using System.Runtime.CompilerServices;
using AIdentities.Shared.Features.CognitiveEngine;
using AIdentities.Shared.Features.CognitiveEngine.Mission;
using AIdentities.Shared.Features.CognitiveEngine.Skills;
using AIdentities.Shared.Features.CognitiveEngine.Thoughts;
using AIdentities.Shared.Plugins.Connectors;

namespace AIdentities.Chat.Skills.IntroduceAIdentity;

public class IntroduceAIdentity : SkillDefinition
{
   public const string NAME = nameof(IntroduceAIdentity);
   const string ACTIVATION_CONTEXT = "The AIdenity has to introduce itself to the conversation";
   const string RETURN_DESCRIPTION = "The sentence that the AIdentity will say to introduce itself";

   const string EXAMPLES = "";

   static readonly JsonSerializerOptions _jsonOptionExample = new() { WriteIndented = true };

   private readonly List<SkillArgumentDefinition> _arguments = new()
   {
      Args.AIdentityIdDefinition
   };

   readonly ILogger<IntroduceAIdentity> _logger;
   readonly IDefaultConnectors _defaultConnectors;
   readonly IPluginStorage<PluginEntry> _pluginStorage;
   readonly IAIdentityProvider _aIdentityProvider;

   public IntroduceAIdentity(ILogger<IntroduceAIdentity> logger,
                             IDefaultConnectors defaultConnectors,
                             IPluginStorage<PluginEntry> pluginStorage,
                             IAIdentityProvider aIdentityProvider
                             )
      : base(NAME, ACTIVATION_CONTEXT, RETURN_DESCRIPTION, EXAMPLES)
   {
      _logger = logger;
      _defaultConnectors = defaultConnectors;
      _pluginStorage = pluginStorage;
      _aIdentityProvider = aIdentityProvider;

      Arguments = _arguments;
   }

   public override async IAsyncEnumerable<Thought> ExecuteAsync(Prompt prompt,
                                                                CognitiveContext cognitiveContext,
                                                                MissionContext? missionContext,
                                                                [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      var connector = _defaultConnectors.DefaultConversationalConnector
         ?? throw new InvalidOperationException("No completion connector is enabled");

      if (!TryExtractFromContext<Args>(cognitiveContext, missionContext, out Args? args))
      {
         if (!TryExtractJson<Args>(prompt.Text, out args))
         {
            yield return cognitiveContext.InvalidPrompt(this);
            yield break;
         }
      }

      if (!Guid.TryParse(args.AIdentityId, out Guid aIdentityId))
      {
         yield return cognitiveContext.MissingArguments(this, Args.AIdentityIdDefinition);
         yield break;
      }


      var aidentity = _aIdentityProvider.Get(aIdentityId);
      if (aidentity is null)
      {
         yield return cognitiveContext.InvalidPrompt(this);
         yield break;
      }


      var streamedResult = connector.RequestChatCompletionAsStreamAsync(new DefaultConversationalRequest
      {
         Messages = PromptTemplates.GetIntroductionPrompt(aidentity),
         MaxGeneratedTokens = 100
      }, cancellationToken).ConfigureAwait(false);

      await foreach (var thought in streamedResult)
      {
         yield return new StreamedFinalThought(Id, aidentity.Id, thought.GeneratedMessage ?? "", false);
      }
      yield return new StreamedFinalThought(Id, aidentity.Id, "", true);
   }
}

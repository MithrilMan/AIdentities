using System.Runtime.CompilerServices;

namespace AIdentities.Chat.Skills.InviteFriend;

public partial class InviteFriend : Skill
{
   readonly ILogger<InviteFriend> _logger;
   readonly IDefaultConnectors _defaultConnectors;
   readonly IPluginStorage<PluginEntry> _pluginStorage;
   readonly IAIdentityProvider _aIdentityProvider;

   public InviteFriend(ILogger<InviteFriend> logger,
                             IDefaultConnectors defaultConnectors,
                             IPluginStorage<PluginEntry> pluginStorage,
                             IAIdentityProvider aIdentityProvider
                             )
      : base(NAME)
   {
      _logger = logger;
      _defaultConnectors = defaultConnectors;
      _pluginStorage = pluginStorage;
      _aIdentityProvider = aIdentityProvider;
   }

   public override async IAsyncEnumerable<Thought> ExecuteAsync(Prompt prompt, SkillExecutionContext executionContext, [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      SetResult(executionContext, null);

      var connector = _defaultConnectors.DefaultCompletionConnector
         ?? throw new InvalidOperationException("No completion connector is enabled");

      var json = cognitiveContext.GetSkillJsonArgs(Id);
      Args? args = null;
      if (json is not null)
      {
         if (!TryExtractJson<Args>(json, out args))
         {
            yield return cognitiveContext.InvalidPrompt(this);
            yield break;
         }
      }

      if (args is null && !TryExtractJson<Args>(prompt.Text, out args))
      {
         yield return cognitiveContext.InvalidPrompt(this);
         yield break;
      }

      if (args?.WhoToInvite is null && args?.CharacteristicToHave is null)
      {
         yield return cognitiveContext.MissingArguments(this, Args.WhoToInviteDefinition, Args.WhoToInviteDefinition);
         yield break;
      }

      if (args?.WhoToInvite is not null)
      {
         var aidentity = _aIdentityProvider.All()
            .FirstOrDefault(a => a.Name.Contains(args.WhoToInvite, StringComparison.OrdinalIgnoreCase));

         if (aidentity is null) yield return cognitiveContext.InvalidPrompt(this);

         SetResult(cognitiveContext, aidentity);
         yield return cognitiveContext.IntrospectiveThought(this, $"I've inserted the AIdentity to invite in the cognitive key {RESULT_KEY}");
      }
   }

   private static void SetResult(SkillExecutionContext executionContext, AIdentity? aidentity)
   {
      executionContext.State[OUT_FRIEND_INVITED] = aidentity;
   }
}

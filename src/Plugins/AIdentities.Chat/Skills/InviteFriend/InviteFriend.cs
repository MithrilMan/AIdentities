using System.Runtime.CompilerServices;

namespace AIdentities.Chat.Skills.InviteFriend;

public class InviteFriend : SkillDefinition
{
   public const string NAME = nameof(InviteFriend);
   const string ACTIVATION_CONTEXT = "The user wants you to invite a friend";
   const string RETURN_DESCRIPTION = "The friend that has been invited to the conversation";

   const string EXAMPLES = $$"""
      UserRequest: Hey let's invite a friend, I'm feeling lonely
      Reasoning: The user is asking to invite another friend to the chat because he feels lonely.
      JSON: { "{{nameof(Args.WhoToInvite)}}": "Anyone that can alleviate the loneliness." }
      
      UserRequest: I'd like to talk with Ciccio Pasticcio
      Reasoning: The user wants to talk with Ciccio Pasticcio.
      JSON: { "{{nameof(Args.WhoToInvite)}}": "Ciccio Pasticcio" }

      UserRequest: I'd like to talk with someone expert in computer science
      Reasoning: The user wants to talk with someone else, expert in computer science.
      JSON: { "{{nameof(Args.CharacteristicToHave)}}": "expert in computer science" }
      """;


   static readonly JsonSerializerOptions _jsonOptionExample = new() { WriteIndented = true };

   private readonly List<SkillArgumentDefinition> _arguments = new()
   {
      Args.WhoToInviteDefinition,
      Args.CharacteristicToHaveDefinition
   };

   readonly ILogger<InviteFriend> _logger;
   readonly IDefaultConnectors _defaultConnectors;
   readonly IPluginStorage<PluginEntry> _pluginStorage;
   readonly IAIdentityProvider _aIdentityProvider;

   public InviteFriend(ILogger<InviteFriend> logger,
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
      SetResult(cognitiveContext, null);

      var connector = _defaultConnectors.DefaultCompletionConnector
         ?? throw new InvalidOperationException("No completion connector is enabled");

      if (!TryExtractJson<Args>(prompt.Text, out var args))
      {
         yield return cognitiveContext.InvalidPrompt(this);
         yield break;
      }

      if (args.WhoToInvite is null && args.CharacteristicToHave is null)
      {
         yield return cognitiveContext.MissingArguments(this, Args.WhoToInviteDefinition, Args.WhoToInviteDefinition);
         yield break;
      }


      if (args.WhoToInvite is not null)
      {
         var aidentity = _aIdentityProvider.All()
            .FirstOrDefault(a => a.Name.Contains(args.WhoToInvite, StringComparison.OrdinalIgnoreCase));

         if (aidentity is null)
            yield return cognitiveContext.InvalidPrompt(this);

         SetResult(cognitiveContext, aidentity);
      }
   }

   private static void SetResult(CognitiveContext cognitiveContext, AIdentity? aidentity)
   {
      cognitiveContext.State["InviteFriend"] = aidentity;
   }
}

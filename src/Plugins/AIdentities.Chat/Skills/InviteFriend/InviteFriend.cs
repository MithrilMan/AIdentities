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
   {
      _logger = logger;
      _defaultConnectors = defaultConnectors;
      _pluginStorage = pluginStorage;
      _aIdentityProvider = aIdentityProvider;
   }

   protected override async IAsyncEnumerable<Thought> ExecuteAsync(
      SkillExecutionContext context,
      CancellationToken cancellationToken)
   {
      SetFriendInvited(context, null);

      var connector = _defaultConnectors.DefaultCompletionConnector
         ?? throw new InvalidOperationException("No completion connector is enabled");

      var whoToInvite = WhoToInvite(context);
      if (whoToInvite is not null)
      {
         var aidentity = _aIdentityProvider.All()
            .FirstOrDefault(a => a.Name.Contains(whoToInvite, StringComparison.OrdinalIgnoreCase));

         if (aidentity is null)
         {
            yield return context.InvalidArgumentsThought($"Cannot find the AIdentity named {whoToInvite}");
         }

         SetFriendInvited(context, aidentity);
         yield return context.IntrospectiveThought( $"I've inserted the AIdentity to invite in the cognitive key {OUT_FRIEND_INVITED}");
      }
   }
}

using System.Runtime.CompilerServices;
using AIdentities.Chat.Skills.InviteFriend.Events;
using AIdentities.Shared.Services.EventBus;

namespace AIdentities.Chat.Skills.InviteFriend;

public partial class InviteFriend : Skill
{
   readonly ILogger<InviteFriend> _logger;
   readonly IAIdentityProvider _aIdentityProvider;
   readonly IEventBus _eventBus;

   public InviteFriend(ILogger<InviteFriend> logger,
                       IAIdentityProvider aIdentityProvider,
                       IEventBus eventBus)
   {
      _logger = logger;
      _aIdentityProvider = aIdentityProvider;
      _eventBus = eventBus;
   }

   protected override async IAsyncEnumerable<Thought> ExecuteAsync(
      SkillExecutionContext context,
      [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      SetFriendInvited(context, null);

      var characteristicsToHave = CharacteristicToHave(context);
      if (characteristicsToHave != null)
      {
         var aidentity = _aIdentityProvider.
            All()
            .FirstOrDefault(a => a.Personality?.Contains(characteristicsToHave, StringComparison.InvariantCultureIgnoreCase) ?? false);

         if (aidentity is null)
         {
            yield return context.InvalidArgumentsThought($"Cannot find the AIdentity with characteristics {characteristicsToHave}");
            yield break;
         }

         SetFriendInvited(context, aidentity);
         yield return context.IntrospectiveThought($"I've inserted the AIdentity to invite in the cognitive key {OUT_FRIEND_INVITED}");

         await _eventBus.PublishAsync(new InviteToConversation(aidentity)).ConfigureAwait(false);
      }
      else
      {
         var whoToInvite = WhoToInvite(context);
         if (whoToInvite is not null)
         {
            var aidentity = _aIdentityProvider.Get(whoToInvite);

            if (aidentity is null)
            {
               yield return context.InvalidArgumentsThought($"Cannot find the AIdentity named {whoToInvite}");
               yield break;
            }

            SetFriendInvited(context, aidentity);
            yield return context.IntrospectiveThought($"I've inserted the AIdentity to invite in the cognitive key {OUT_FRIEND_INVITED}");

            await _eventBus.PublishAsync(new InviteToConversation(aidentity)).ConfigureAwait(false);
         }
      }
   }
}

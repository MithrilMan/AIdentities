using AIdentities.Shared.Services.EventBus;
using Fluid;

namespace AIdentities.Chat.Skills.InviteToChat;

public partial class InviteToChat : Skill
{
   readonly IEventBus _eventBus;

   public InviteToChat(ILogger<InviteToChat> logger,
                       IAIdentityProvider aIdentityProvider,
                       FluidParser templateParser,
                       IEventBus eventBus)
      : base(logger, aIdentityProvider, templateParser)
   {
      _eventBus = eventBus;
   }

   protected override void CreateDefaultPromptTemplates() { }

   protected override async IAsyncEnumerable<Thought> ExecuteAsync(
      SkillExecutionContext context,
      [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      SetFriendInvited(context, null);

      var characteristicsToHave = CharacteristicToHave(context);
      if (characteristicsToHave != null)
      {
         var aidentity = AIdentityProvider
            .All()
            .FirstOrDefault(a => a.Personality?.Contains(characteristicsToHave, StringComparison.InvariantCultureIgnoreCase) ?? false);

         if (aidentity is null)
         {
            yield return context.InvalidArgumentsThought($"Cannot find the AIdentity with characteristics {characteristicsToHave}");
            yield break;
         }

         SetFriendInvited(context, aidentity);
         yield return context.IntrospectiveThought($"I've inserted the AIdentity to invite in the cognitive key {OUT_FRIEND_INVITED}");

         await _eventBus.PublishAsync(new Events.InviteToConversation(aidentity)).ConfigureAwait(false);
      }
      else
      {
         var whoToInvite = WhoToInvite(context);
         if (whoToInvite is not null)
         {
            var aidentity = AIdentityProvider.Get(whoToInvite);

            if (aidentity is null)
            {
               yield return context.InvalidArgumentsThought($"Cannot find the AIdentity named {whoToInvite}");
               yield break;
            }

            SetFriendInvited(context, aidentity);
            yield return context.IntrospectiveThought($"I've inserted the AIdentity to invite in the cognitive key {OUT_FRIEND_INVITED}");

            await _eventBus.PublishAsync(new Events.InviteToConversation(aidentity)).ConfigureAwait(false);
         }
      }
   }
}

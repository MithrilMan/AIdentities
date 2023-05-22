namespace AIdentities.Chat.Skills.InviteFriend;

[SkillDefinition(
   Name = NAME, SkillType = typeof(InviteFriend),
   ActivationContext = "The user wants you to invite a friend"
   )]
[SkillInputDefinition(
   Name = IN_WHO_TO_INVITE, Type = SkillVariableType.String,
   IsRequired = false,
   Description = "Who the user wants to talk with/invite."
   )]
[SkillInputDefinition(
   Name = IN_CHARACTERISTIC_TO_HAVE, Type = SkillVariableType.String,
   IsRequired = false,
   Description = "Which characteristic the friend must have in order to be invited."
   )]
[SkillOutputDefinition(
   Name = OUT_FRIEND_INVITED, Type = SkillVariableType.Guid,
   Description = "The Id of the AIdentity that has been invited to the conversation"
   )]
[SkillExample(Example = """
   UserRequest: Hey let's invite a friend, I'm feeling lonely
   Reasoning: The user is asking to invite another friend to the chat because he feels lonely.
   JSON: { "WhoToInvite": "Anyone that can alleviate the loneliness." }
   """)]
[SkillExample(Example = """
   UserRequest: I'd like to talk with Ciccio Pasticcio
   Reasoning: The user wants to talk with Ciccio Pasticcio.
   JSON: { "WhoToInvite": "Ciccio Pasticcio" }
   """)]
[SkillExample(Example = """
   UserRequest: I'd like to talk with someone expert in computer science
   Reasoning: The user wants to talk with someone else, expert in computer science.
   JSON: { "CharacteristicToHave": "expert in computer science" }
   """)]
public partial class InviteFriend : Skill
{
   const string NAME = nameof(InviteFriend);
   const string IN_WHO_TO_INVITE = "WhoToInvite";
   const string IN_CHARACTERISTIC_TO_HAVE = "CharacteristicToHave";
   const string OUT_FRIEND_INVITED = "FriendInvited";
}

using System.Diagnostics.CodeAnalysis;

namespace AIdentities.Chat.Skills.InviteFriend;

[SkillDefinition(
   Name = NAME,
   Description = "The user wants you to invite a friend",
   Tags = new[] { SkillTags.TAG_CHAT }
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
   Name = OUT_FRIEND_INVITED, Type = SkillVariableType.String,
   Description = "The Id of the AIdentity that has been invited to the conversation"
   )]
[SkillExample(Example = """
   UserRequest: Hey let's invite a friend, I'm feeling lonely
   Reasoning: The user is asking to invite another friend to the chat because he feels lonely.
   JSON: { "CharacteristicToHave": "Anyone that can alleviate the loneliness." }
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
   const string IN_WHO_TO_INVITE = nameof(WhoToInvite);
   const string IN_CHARACTERISTIC_TO_HAVE = nameof(CharacteristicToHave);
   const string OUT_FRIEND_INVITED = nameof(SetFriendInvited);

   public static string? WhoToInvite(SkillExecutionContext context)
      => context.GetInput<string>(IN_WHO_TO_INVITE);

   public static string? CharacteristicToHave(SkillExecutionContext context)
      => context.GetInput<string>(IN_CHARACTERISTIC_TO_HAVE);

   private static void SetFriendInvited(SkillExecutionContext context, AIdentity? aidentity)
      => context.SetOutput(OUT_FRIEND_INVITED, aidentity?.Id);

   protected override bool ValidateInputs(Prompt prompt, SkillExecutionContext context, [MaybeNullWhen(true)] out InvalidArgumentsThought error)
   {
      string? whoToInvite;
      try
      {
         whoToInvite = WhoToInvite(context);
      }
      catch (Exception ex)
      {
         error = context.InvalidArgumentsThought(ex.Message);
         return false;
      }

      string? characteristicToHave;
      try
      {
         characteristicToHave = CharacteristicToHave(context);
      }
      catch (Exception ex)
      {
         error = context.InvalidArgumentsThought(ex.Message);
         return false;
      }

      //at least one of the two must be present
      if (whoToInvite is null && characteristicToHave is null)
      {
         if (TryExtractJson<dynamic>(prompt.Text, out var args))
         {
            context.SetInput(IN_WHO_TO_INVITE, SkillVariableType.String, args.WhoToInvite);
            context.SetInput(IN_CHARACTERISTIC_TO_HAVE, SkillVariableType.String, args.CharacteristicToHave);
         }
      }

      error = null;
      return true;
   }
}

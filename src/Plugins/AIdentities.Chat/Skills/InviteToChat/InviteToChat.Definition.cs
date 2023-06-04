using System.Diagnostics.CodeAnalysis;

namespace AIdentities.Chat.Skills.InviteToChat;

[SkillDefinition(
   Name = NAME,
   Description = "The user wants you to invite someone to the conversation",
   Tags = new[] { SkillTags.TAG_CHAT }
   )]
[SkillInputDefinition(
   Name = IN_WHO_TO_INVITE, Type = SkillVariableType.String,
   IsRequired = false,
   Description = "The name of the person the user wants to invite to the conversation."
   )]
[SkillInputDefinition(
   Name = IN_CHARACTERISTIC_TO_HAVE, Type = SkillVariableType.String,
   IsRequired = false,
   Description = "Which characteristic the person must have in order to be invited to the conversation."
   )]
[SkillOutputDefinition(
   Name = OUT_FRIEND_INVITED, Type = SkillVariableType.String,
   Description = "The Id of the AIdentity that has been invited to the conversation"
   )]
[SkillExample(
   UserRequest = "Hey let's invite someone, I'm feeling lonely",
   Reasoning = "The user is asking to invite someone to the chat because he feels lonely.",
   JsonExample = """{ "CharacteristicToHave": "Anyone that can alleviate the loneliness." }"""
   )]
[SkillExample(
   UserRequest = "I'd like to talk with Ciccio Pasticcio",
   Reasoning = "The user wants to talk with someone called Ciccio Pasticcio.",
   JsonExample = """{ "WhoToInvite": "Ciccio Pasticcio" }"""
   )]
[SkillExample(
   UserRequest = "I'd like to talk with someone expert in computer science",
   Reasoning = "The user wants to talk with someone expert in computer science.",
   JsonExample = """{ "CharacteristicToHave": "expert in computer science" }"""
   )]
public partial class InviteToChat : Skill
{
   const string NAME = nameof(InviteToChat);
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

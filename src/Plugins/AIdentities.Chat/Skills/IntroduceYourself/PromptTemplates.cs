using System.Text;

namespace AIdentities.Chat.Skills.IntroduceYourself;

internal static class PromptTemplates
{
   public static IEnumerable<ConversationalMessage> GetIntroductionPrompt(AIdentity aIdentity, IEnumerable<string> participants)
   {
      var chatFeature = aIdentity.Features.Get<AIdentityChatFeature>();
      var background = chatFeature?.Background;
      var personality = aIdentity.Personality;

      //var sb = new StringBuilder($"""
      //   You are {aIdentity.Name} and you are participating to a new conversation.
      //   Based on your trait listed below, write a starting sentence to introduce yourself into the conversation that started already and you don't know nothing about.
      //   <PERSONALITY>{aIdentity.Personality}</PERSONALITY>

      //   """);

      var sb = new StringBuilder()
         .AppendLine($"You are {aIdentity.Name} and you are participating to a conversation between you and {PromptTokens.TOKEN_PARTICIPANTS}");

      if (background != null)
      {
         sb.AppendLine($"Personal background: {background.AsSingleLine()}");
      }

      if (personality != null)
      {
         sb.AppendLine($"Personality: {personality.AsSingleLine()}");
      }

      sb.AppendLine($"""
      Goal: Based on your characteristics above, chime in saying something briefly that fits your personality.
      """)
         .AppendLine(PromptUtils.BuildAIdentityMessageStyleExamples(chatFeature, aIdentity.Name ?? "Assistant"))
         .AppendLine($"Roleplay between you and {PromptTokens.TOKEN_PARTICIPANTS} begins. Talk only on behalf of {PromptTokens.TOKEN_AIDENTITY_NAME}.");
      ;

      sb.Replace(PromptTokens.TOKEN_PARTICIPANTS, PromptUtils.BuildParticipants(aIdentity, participants))
         .Replace(PromptTokens.TOKEN_AIDENTITY_NAME, aIdentity.Name);

      yield return new ConversationalMessage(
         Content: sb.ToString(),
         Role: ConversationalRole.System,
         Name: aIdentity.Name
      );

      //yield return new DefaultConversationalMessage(
      //   Content: $"Hey it seems someone is joining us, look, it's {aIdentity.Name}",
      //   Role: DefaultConversationalRole.User,
      //   Name: "Guest"
      //   );
   }
}

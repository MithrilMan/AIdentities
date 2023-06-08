using System.Text;
using Bogus;

namespace AIdentities.Chat.Skills.IntroduceYourself;

internal static class PromptTemplates
{
   public static IEnumerable<DefaultConversationalMessage> GetIntroductionPrompt(AIdentity aIdentity)
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
         .AppendLine($"You are {aIdentity.Name} and you are participating to a new conversation.");

      if (background != null)
      {
         sb.AppendLine($"Personal background: {background
            .Replace("\r\n", "")
            .Replace("\n", "")
            }");
      }

      if (personality != null)
      {
         sb.AppendLine($"Personality: {personality
            .Replace("\r\n", "")
            .Replace("\n", "")
            }");
      }

      sb.AppendLine($"""
      Goal: Based on your characteristics above, chime in saying something briefly that fits your personality.
      """)
         .AppendLine(ReplyToPrompt.PromptTemplates.BuildExamples(chatFeature, aIdentity.Name ?? "Assistant"))
         .AppendLine("Roleplay begins, you are joining a conversation between you and other characters:");

      yield return new DefaultConversationalMessage(
         Content: sb.ToString(),
         Role: DefaultConversationalRole.System,
         Name: aIdentity.Name
      );

      yield return new DefaultConversationalMessage(
         Content: $"Hey it seems someone is joining us, look, it's {aIdentity.Name}",
         Role: DefaultConversationalRole.User,
         Name: "Guest"
         );
   }
}

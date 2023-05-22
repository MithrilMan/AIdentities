using System.Text;

namespace AIdentities.Chat.Skills.IntroduceYourself;

internal static class PromptTemplates
{
   public static IEnumerable<DefaultConversationalMessage> GetIntroductionPrompt(AIdentity aIdentity)
   {
      var background = aIdentity.Features.Get<AIdentityChatFeature>()?.Background;

      var sb = new StringBuilder($"""
         You are {aIdentity.Name} and you are partecipating to a new conversation with other people.
         Based on your trait listed below, write a starting sentence to introduce yourself into the conversation that started already and you don't know nothing about.
         <PERSONALITY>{aIdentity.Personality}</PERSONALITY>

         """);

      if (background != null)
      {
         sb.Append($"""
         <BACKGROUND>{background}</BACKGROUND>

         """);
      }


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

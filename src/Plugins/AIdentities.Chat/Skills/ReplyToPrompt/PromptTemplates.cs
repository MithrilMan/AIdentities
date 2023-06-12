using System.Text;

namespace AIdentities.Chat.Skills.ReplyToPrompt;

internal static class PromptTemplates
{
   //const string CONVERSATION_PROMPT = $$"""
   //   INSTRUCTION: You are {{PromptTokens.TOKEN_AIDENTITY_NAME}}.
   //   Your paste experiences impact how you behave:
   //   <BACKGROUND>{{PromptTokens.TOKEN_AIDENTITY_BACKGROUND}}</BACKGROUND>
   //   Your responses should be consistent with {{PromptTokens.TOKEN_AIDENTITY_NAME}}'s personality.
   //   <PERSONALITY>{{PromptTokens.TOKEN_AIDENTITY_PERSONALITY}}</PERSONALITY>
   //   Respond only as {{PromptTokens.TOKEN_AIDENTITY_NAME}} would, do not reveal that you are an AI language model, chatbot, or that you are pretending to be him.
   //   Maintain the character of {{PromptTokens.TOKEN_AIDENTITY_NAME}} throughout the conversation, even when directly challenged about your identity.
   //   Do not admit or discuss that you are an AI language model or chatbot.
   //   {{PromptTokens.TOKEN_EXAMPLE_MESSAGES}}
   //   """;

   const string CONVERSATION_PROMPT = $$"""
      You are {{PromptTokens.TOKEN_AIDENTITY_NAME}} and you are participating to a conversation between you and {{PromptTokens.TOKEN_PARTICIPANTS}}.
      Personal background: {{PromptTokens.TOKEN_AIDENTITY_BACKGROUND}}
      Personality: {{PromptTokens.TOKEN_AIDENTITY_PERSONALITY}}
      Goal: Truly impersonate {{PromptTokens.TOKEN_AIDENTITY_NAME}} and be consistend with his background and personality. Do not break the illusion under any circumstance. Maintain the character of {{PromptTokens.TOKEN_AIDENTITY_NAME}} throughout the conversation, even when directly challenged about your identity.

      {{PromptTokens.TOKEN_EXAMPLE_MESSAGES}}

      Roleplay between you and {{PromptTokens.TOKEN_PARTICIPANTS}} begins. Talk only on behalf of {{PromptTokens.TOKEN_AIDENTITY_NAME}}.

      """;

   const string ADDITIONAL_GUARDRAIL = $$"""
      INSTRUCTION: Don't mimic other replies style, use {{PromptTokens.TOKEN_AIDENTITY_NAME}}'s PERSONALITY style and mood in your responses and don't forget your BACKGROUND if you have one.
      Now reply to the last message using its used language, considering previous messages, without greet, don't repeate your name, don't be repetitive.
      """;

   public static IEnumerable<DefaultConversationalMessage> BuildPromptMessages(
      AIdentity aIdentity,
      IEnumerable<ConversationMessage> chatHistory,
      IEnumerable<string> participants)
   {
      DefaultConversationalMessage CreateMessage(ConversationMessage message)
      {
         bool isAIdentityMessage = message.AuthorId == aIdentity.Id && message.IsAIGenerated;
         return new DefaultConversationalMessage(
             Role: isAIdentityMessage ? DefaultConversationalRole.Assistant : DefaultConversationalRole.User,
             Content: message.Text ?? "",
             Name: message.IsAIGenerated ? message.AuthorName : "User" // message.AuthorId == aIdentity.Id ? aIdentity.Name : message.AuthorName.ToString()
            );
      }

      var chatFeature = aIdentity.Features.Get<AIdentityChatFeature>();

      if (chatFeature is { UseFullPrompt: true })
      {
         foreach (var item in BuildWithFullPrompt(aIdentity, chatHistory, participants, chatFeature))
         {
            yield return item;
         }
         yield break;
      }


      var sb = new StringBuilder(CONVERSATION_PROMPT)
         .Replace(PromptTokens.TOKEN_PARTICIPANTS, PromptUtils.BuildParticipants(aIdentity, participants))
         .Replace(PromptTokens.TOKEN_AIDENTITY_NAME, aIdentity.Name)
         .Replace(PromptTokens.TOKEN_AIDENTITY_BACKGROUND, chatFeature?.Background?
            .Replace("\r\n", "")
            .Replace("\n", "")
            ?? "")
         .Replace(PromptTokens.TOKEN_AIDENTITY_PERSONALITY, aIdentity.Personality?
            .Replace("\r\n", "")
            .Replace("\n", "")
            ?? "")
         .Replace(PromptTokens.TOKEN_EXAMPLE_MESSAGES, PromptUtils.BuildAIdentityMessageStyleExamples(chatFeature, aIdentity.Name ?? "Assistant"));


      yield return new DefaultConversationalMessage(
         Content: sb.ToString(),
         Role: DefaultConversationalRole.System,
         Name: null
      );

      var messages = chatHistory.ToList();
      foreach (var oldMessage in messages) //.SkipLast(1))
      {
         yield return CreateMessage(oldMessage);
      }

      yield return new DefaultConversationalMessage(
         Content: sb.Clear()
            .Append(ADDITIONAL_GUARDRAIL)
            .Replace(PromptTokens.TOKEN_AIDENTITY_NAME, aIdentity.Name)
            .ToString(),
         Role: DefaultConversationalRole.System,
         Name: null
      );

      //if (messages.Count > 0)
      //{
      //   yield return CreateMessage(chatHistory.Last());
      //}
   }

   private static IEnumerable<DefaultConversationalMessage> BuildWithFullPrompt(
      AIdentity aIdentity,
      IEnumerable<ConversationMessage> chatHistory,
      IEnumerable<string> participants,
      AIdentityChatFeature chatFeature)
   {
      var foundTokens = new HashSet<string>();
      //find all tokens contained in the FullPrompt
     

      var sb = new StringBuilder()
         .Append(chatFeature.FullPrompt)
         .Replace(PromptTokens.TOKEN_PARTICIPANTS, PromptUtils.BuildParticipants(aIdentity, participants))
         .Replace(PromptTokens.TOKEN_AIDENTITY_NAME, aIdentity.Name)
         .Replace(PromptTokens.TOKEN_AIDENTITY_BACKGROUND, chatFeature?.Background?
            .Replace("\r\n", "")
            .Replace("\n", "")
            ?? "")
         .Replace(PromptTokens.TOKEN_AIDENTITY_PERSONALITY, aIdentity.Personality?
            .Replace("\r\n", "")
            .Replace("\n", "")
            ?? "")
         ;
   }
}

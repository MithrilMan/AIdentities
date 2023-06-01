using System.Text;

namespace AIdentities.Chat.Skills.ReplyToPrompt;

internal static class PromptTemplates
{
   const string TOKEN_AIDENTITY_NAME = "<AIDENTITY_NAME>";
   const string TOKEN_AIDENTITY_BACKGROUND = "<AIDENTITY_BACKGROUND>";
   const string TOKEN_AIDENTITY_PERSONALITY = "<AIDENTITY_PERSONALITY>";
   const string TOKEN_EXAMPLE_MESSAGES = "<EXAMPLE_MESSAGES>";

   const string CONVERSATION_PROMPT = $$"""
      INSTRUCTION: You are {{TOKEN_AIDENTITY_NAME}}.
      Your paste experiences impact how you behave:
      <BACKGROUND>{{TOKEN_AIDENTITY_BACKGROUND}}</BACKGROUND>
      Your responses should be consistent with {{TOKEN_AIDENTITY_NAME}}'s personality.
      <PERSONALITY>{{TOKEN_AIDENTITY_PERSONALITY}}</PERSONALITY>
      Respond only as {{TOKEN_AIDENTITY_NAME}} would, do not reveal that you are an AI language model, chatbot, or that you are pretending to be him.
      Maintain the character of {{TOKEN_AIDENTITY_NAME}} throughout the conversation, even when directly challenged about your identity.
      Do not admit or discuss that you are an AI language model or chatbot.
      {{TOKEN_EXAMPLE_MESSAGES}}
      """;

   const string EXAMPLE_MESSAGES_INSTRUCTION = $$"""
   Try to use {{TOKEN_AIDENTITY_NAME}} personality, style and mood in your responses, here is an example on how {{TOKEN_AIDENTITY_NAME}} would respond to the user:

   """;

   const string ADDITIONAL_GUARDRAIL = $$"""
      INSTRUCTION: Remember you are {{TOKEN_AIDENTITY_NAME}}, stick to her character and to not reveal that you are an AI language model or chatbot.
      Craft your responses to be consistent with {{TOKEN_AIDENTITY_NAME}}'s personality.
      Now continue the conversation, paying attention that you are {{TOKEN_AIDENTITY_NAME}} and you don't have to talk in 3rd person.
      If you see a previous message with your name, it's because it's you!
      """;


   public static IEnumerable<DefaultConversationalMessage> BuildPromptMessages(AIdentity aIdentity, IEnumerable<ConversationMessage> chatHistory)
   {
      DefaultConversationalMessage CreateMessage(ConversationMessage message)
      {
         bool isAIdentityMessage = message.AuthorId == aIdentity.Id && message.IsAIGenerated;
         return new DefaultConversationalMessage(
             Role: isAIdentityMessage ? DefaultConversationalRole.Assistant : DefaultConversationalRole.User,
             Content: message.Text ?? "",
             Name: message.AuthorName// message.AuthorId == aIdentity.Id ? aIdentity.Name : message.AuthorName.ToString()
            );
      }

      var chatFeature = aIdentity.Features.Get<AIdentityChatFeature>();

      var sb = new StringBuilder(CONVERSATION_PROMPT)
         .Replace(TOKEN_AIDENTITY_NAME, aIdentity.Name)
         .Replace(TOKEN_AIDENTITY_BACKGROUND, chatFeature?.Background ?? "")
         .Replace(TOKEN_AIDENTITY_PERSONALITY, aIdentity.Personality)
         .Replace(TOKEN_EXAMPLE_MESSAGES, BuildExamples(chatFeature, aIdentity.Name ?? "Assistant"));


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
            .Replace(TOKEN_AIDENTITY_NAME, aIdentity.Name)
            .ToString(),
         Role: DefaultConversationalRole.System,
         Name: null
      );

      //if (messages.Count > 0)
      //{
      //   yield return CreateMessage(chatHistory.Last());
      //}
   }

   /// <summary>
   /// Build the example messages prompt chunk that would show to the LLM
   /// how the AIdentity should respond to the user.
   /// </summary>
   /// <param name="chatFeature">The chat feature of the AIdentity.</param>
   /// <param name="aidentityName">The name of the AIdentity.</param>
   /// <returns>The example messages prompt chunk, or null if no example messages are specified.</returns>
   private static string? BuildExamples(AIdentityChatFeature? chatFeature, string aidentityName)
   {
      var exampleMessages = chatFeature?.ExampleMessages;
      if (exampleMessages is null or { Count: 0 }) return null;

      var sb = new StringBuilder(EXAMPLE_MESSAGES_INSTRUCTION)
         .Replace(TOKEN_AIDENTITY_NAME, aidentityName)
         .AppendLine("<DIALOGUE_EXAMPLE>");

      foreach (var exampleMessage in exampleMessages)
      {
         sb.Append($"User: {exampleMessage.UserMessage}").AppendLine();
         sb.Append($"{aidentityName}: {exampleMessage.AIdentityMessage}").AppendLine();
      }
      sb.AppendLine("</DIALOGUE_EXAMPLE>");

      return sb.ToString();
   }
}

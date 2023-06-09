using System.Text;

namespace AIdentities.Chat.Skills.ReplyToPrompt;

internal static class PromptTemplates
{
   const string TOKEN_AIDENTITY_NAME = "<AIDENTITY_NAME>";
   const string TOKEN_AIDENTITY_BACKGROUND = "<AIDENTITY_BACKGROUND>";
   const string TOKEN_AIDENTITY_PERSONALITY = "<AIDENTITY_PERSONALITY>";
   const string TOKEN_EXAMPLE_MESSAGES = "<EXAMPLE_MESSAGES>";
   const string TOKEN_PARTICIPANTS = "<PARTICIPANTS>";

   //const string CONVERSATION_PROMPT = $$"""
   //   INSTRUCTION: You are {{TOKEN_AIDENTITY_NAME}}.
   //   Your paste experiences impact how you behave:
   //   <BACKGROUND>{{TOKEN_AIDENTITY_BACKGROUND}}</BACKGROUND>
   //   Your responses should be consistent with {{TOKEN_AIDENTITY_NAME}}'s personality.
   //   <PERSONALITY>{{TOKEN_AIDENTITY_PERSONALITY}}</PERSONALITY>
   //   Respond only as {{TOKEN_AIDENTITY_NAME}} would, do not reveal that you are an AI language model, chatbot, or that you are pretending to be him.
   //   Maintain the character of {{TOKEN_AIDENTITY_NAME}} throughout the conversation, even when directly challenged about your identity.
   //   Do not admit or discuss that you are an AI language model or chatbot.
   //   {{TOKEN_EXAMPLE_MESSAGES}}
   //   """;

   const string CONVERSATION_PROMPT = $$"""
      You are {{TOKEN_AIDENTITY_NAME}} and you are participating to a conversation between you and {{TOKEN_PARTICIPANTS}}.
      Personal background: {{TOKEN_AIDENTITY_BACKGROUND}}
      Personality: {{TOKEN_AIDENTITY_PERSONALITY}}
      Goal: Truly impersonate {{TOKEN_AIDENTITY_NAME}} and be consistend with his background and personality. Do not break the illusion under any circumstance. Maintain the character of {{TOKEN_AIDENTITY_NAME}} throughout the conversation, even when directly challenged about your identity.

      {{TOKEN_EXAMPLE_MESSAGES}}

      Roleplay between you and other characters begins. Talk only on behalf of {{TOKEN_AIDENTITY_NAME}}.

      """;

   const string EXAMPLE_MESSAGES_INSTRUCTION = $"This is how {TOKEN_AIDENTITY_NAME} should talk";

   const string ADDITIONAL_GUARDRAIL = $$"""
      INSTRUCTION: Don't mimic other replies style, use {{TOKEN_AIDENTITY_NAME}}'s PERSONALITY style and mood in your responses and don't forget your BACKGROUND if you have one.
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

      var sb = new StringBuilder(CONVERSATION_PROMPT)
         .Replace(TOKEN_PARTICIPANTS, BuildParticipants(aIdentity, participants))
         .Replace(TOKEN_AIDENTITY_NAME, aIdentity.Name)
         .Replace(TOKEN_AIDENTITY_BACKGROUND, chatFeature?.Background?
            .Replace("\r\n", "")
            .Replace("\n", "")
            ?? "")
         .Replace(TOKEN_AIDENTITY_PERSONALITY, aIdentity.Personality?
            .Replace("\r\n", "")
            .Replace("\n", "")
            ?? "")
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

   private static string BuildParticipants(AIdentity aIdentity, IEnumerable<string> participants)
   {
      participants = participants.Where(p => p != aIdentity.Name);

      if (!participants.Any()) return "other characters";

      var participantCount = participants.Count();
      if (participantCount == 1) return $"another character: {participants.First()}";

      var sb = new StringBuilder($"{participantCount + 1} characters: ");
      sb.Append(string.Join(", ", participants));

      return sb.ToString();
   }

   /// <summary>
   /// Build the example messages prompt chunk that would show to the LLM
   /// how the AIdentity should respond to the user.
   /// </summary>
   /// <param name="chatFeature">The chat feature of the AIdentity.</param>
   /// <param name="aidentityName">The name of the AIdentity.</param>
   /// <returns>The example messages prompt chunk, or null if no example messages are specified.</returns>
   public static string? BuildExamples(AIdentityChatFeature? chatFeature, string aidentityName)
   {
      var exampleMessages = chatFeature?.ExampleMessages;
      if (exampleMessages is null or { Count: 0 }) return null;

      var sb = new StringBuilder()
         .AppendLine(EXAMPLE_MESSAGES_INSTRUCTION)
         .Replace(TOKEN_AIDENTITY_NAME, aidentityName);

      foreach (var exampleMessage in exampleMessages)
      {
         sb.AppendLine($"User: {exampleMessage.UserMessage}");
         sb.AppendLine($"{aidentityName}: {exampleMessage.AIdentityMessage}");
      }

      return sb.ToString();
   }
}
